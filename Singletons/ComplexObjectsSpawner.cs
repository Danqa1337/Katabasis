using Assets.Scripts;
using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ComplexObjectsSpawner : SpawnerUtility
{
    public Entity Spawn(ComplexObjectData complexObjectData, TileData currentTile)
    {
        try
        {
            if (complexObjectData.Body == null) throw new Exception("Complex object has no body!");

            var body = Spawner.Spawn(complexObjectData.Body, currentTile);
            var complexObjectName = complexObjectData.ComplexObjectName;
            currentTile = currentTile.Refresh();
            if (!Application.isPlaying)
            {
                return body;
            }
            else
            {
                body.AddComponentData(new ComplexObjectNameComponent(complexObjectName));
                body.AddComponentData(new LocalToWorld());
                body.AddComponentData(new AnatomyComponent(body));

                body.AddComponentData(new InventoryComponent(body));
                body.AddComponentData(new PathComponent(true));

                body.AddBuffer<Child>();

                AddComponent(body, complexObjectData.squadMemberComponent);
                AddComponent(body, complexObjectData.creatureComponent);
                AddComponent(body, complexObjectData.moraleComponent);
                AddComponent(body, complexObjectData.equipmentComponent);

                AddBuffer(body, complexObjectData.availableAbilities);
                AddBuffer(body, complexObjectData.enemyTagElements);
                AddBuffer(body, complexObjectData.missingBodyparts);
                AddBuffer(body, complexObjectData.perks);

                if (complexObjectData.alive)
                {
                    body.AddComponentData(new AliveTag());
                    body.AddComponentData(new AIComponent(body, 0, currentTile.position));
                }

                if (complexObjectData.hasAI)
                {
                    body.AddComponentData(new AIComponent(body, 0, currentTile.position));
                }

                if (complexObjectData.squadMemberComponent.IsValid)
                {
                    Registers.SquadsRegister.MoveToSquad(complexObjectData.squadMemberComponent.Component.squadIndex, body);
                }
                else
                {
                    body.AddComponentData(new SquadMemberComponent());
                    Registers.SquadsRegister.MoveToSquad(Registers.SquadsRegister.RegisterNewSquad(), body);
                }

                for (int i = 0; i < complexObjectData.BodyParts.Length; i++)
                {
                    if (complexObjectData.BodyParts[i] != null)
                    {
                        var newPartEntity = Spawner.Spawn(complexObjectData.BodyParts[i], currentTile);
                        body.AddBufferElement(new AnatomyChangeElement(newPartEntity, body));
                        currentTile.Remove(newPartEntity);
                    }
                }

                if (complexObjectData.itemsInInventory != null)
                {
                    var inventory = body.GetComponentData<InventoryComponent>();
                    foreach (var item in complexObjectData.itemsInInventory)
                    {
                        var itemEntity = Spawner.Spawn(item, currentTile);
                        currentTile.Remove(itemEntity);
                        body.AddBufferElement(new ChangeInventoryElement(itemEntity, true));
                    }
                }
                body.AddComponentData(new WalkabilityDataComponent(body));

                AddEquip();
                currentTile.Save();

                void AddEquip()
                {
                    var equip = body.GetComponentData<EquipmentComponent>();

                    if (complexObjectData.itemInMainHand != null)
                    {
                        var item = Spawner.Spawn(complexObjectData.itemInMainHand, currentTile);
                        body.AddBufferElement(new ChangeEquipmentElement(item, EquipTag.Weapon));
                    }
                    if (complexObjectData.itemInOffHand != null)
                    {
                        var item = Spawner.Spawn(complexObjectData.itemInOffHand, currentTile);
                        body.AddBufferElement(new ChangeEquipmentElement(item, EquipTag.Shield));
                    }
                    if (complexObjectData.itemOnHead != null)
                    {
                        var item = Spawner.Spawn(complexObjectData.itemOnHead, currentTile);
                        body.AddBufferElement(new ChangeEquipmentElement(item, EquipTag.Headwear));
                    }
                    if (complexObjectData.itemOnChest != null)
                    {
                        var item = Spawner.Spawn(complexObjectData.itemOnChest, currentTile);
                        body.AddBufferElement(new ChangeEquipmentElement(item, EquipTag.Chestplate));
                    }

                    //if (tags.Contains(Tag.Trader))
                    //{
                    //    //entity.AddComponentData(new TraderComponent() { self = entity });
                    //    //entity.AddBuffer<TradingItemElement>();

                    //    //for (int i = 0; i < 5; i++)
                    //    //{
                    //    //    var tradingItem = Spawn(ObjectDataFactory.GetRandomItem(10, tags: new List<Tag> { Tag.Crafted }), currentTile);
                    //    //    var element = new TradingItemElement(tradingItem, 10);
                    //    //    currentTile.Remove(tradingItem);
                    //    //    entity.GetBuffer<TradingItemElement>().Add(element);
                    //    //    if (i == 0)
                    //    //    {
                    //    //        entity.AddBufferElement(new ChangeEquipmentElement(tradingItem, EquipTag.Weapon));
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        entity.AddBufferElement(new ChangeInventoryElement(tradingItem, true));
                    //    //    }
                    //    //}
                    //    //entity.AddBufferElement(new ChangeOverHeadAnimationElement(OverHeadAnimationType.Dialog, true));
                    //}
                }
            }
            return body;
        }
        catch (Exception exception)
        {
            Debug.Log("Failed to spawn " + complexObjectData.ComplexObjectName + ": " + exception.ToString());
            throw exception;
        }
    }
}
