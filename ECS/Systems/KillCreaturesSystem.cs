using Assets.Scripts;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class KillCreaturesSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        var currentTileFromEntity = GetComponentLookup<CurrentTileComponent>(true);
        Entities.WithAll<KillCreatureComponent, AliveTag>().ForEach(
            (
            Entity entity,
            ref ObjectTypeComponent objectTypeComponent,
            ref CreatureComponent creatureComponent,
            ref EquipmentComponent equipmentComponent,
            ref KillCreatureComponent killCreatureComponent,
            in CurrentTileComponent currentTileComponent,
            in SimpleObjectNameComponent iDComponent,
            in DynamicBuffer<InventoryBufferElement> inventoryBufferElements) =>
        {
            var currentTile = currentTileComponent.currentTileId.ToTileData();
            ecb.RemoveComponent<AIComponent>(entity);
            ecb.RemoveComponent<SquadMemberComponent>(entity);

            if (entity.HasBuffer<MoraleChangeElement>())
            {
                ecb.RemoveComponent<MoraleChangeElement>(entity);
            }

            if (!entity.IsPlayer())
            {
                var squadmates = Registers.SquadsRegister.GetSquadmates(entity);

                ecb.RemoveComponent<AliveTag>(entity);

                foreach (var item in squadmates)
                {
                    if (currentTileFromEntity[item].currentTileId.ToTileData().GetSqrDistance(currentTile) <= 16)
                    {
                        ecb.AddBufferElement(item, new MoraleChangeElement(-40f / squadmates.Count));
                    }
                }
                Registers.SquadsRegister.RemoveFromAnySquads(entity);

                if (killCreatureComponent.responsibleEntity.IsPlayersSquadmate() || killCreatureComponent.responsibleEntity.IsPlayer())
                {
                    PlayerXPHandler.AddXP(creatureComponent.xpOnDeath);
                }
                if (entity.HasComponent<DecayableTag>() && !entity.HasComponent<DecayComponent>())
                {
                    ecb.AddComponent(entity, new DecayComponent(DecaySystem.baseDecayTime));
                }
                foreach (var item in entity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
                {
                    if (item.HasComponent<DecayableTag>() && !item.HasComponent<DecayComponent>())
                    {
                        ecb.AddComponent(item, new DecayComponent(DecaySystem.baseDecayTime));
                    }
                }
                foreach (var item in inventoryBufferElements)
                {
                    ecb.AddBuffer<ChangeInventoryElement>(entity);
                    ecb.AppendToBuffer(entity, new ChangeInventoryElement(item.entity, false));
                }

                var equipedItems = equipmentComponent.GetEquipmentNotNull();
                if (equipedItems.Count > 0)
                {
                    ecb.AddBuffer<ChangeEquipmentElement>(entity);
                    foreach (var item in equipmentComponent.GetEquipmentNotNull())
                    {
                        var equipTag = equipmentComponent.GetEquipTag(item);
                        ecb.AppendToBuffer(entity, new ChangeEquipmentElement(Entity.Null, equipTag));
                        ecb.AddComponent(item, new MoveComponent(currentTile, currentTile, MovemetType.Forced));
                    }
                }
                if (entity.HasComponent<FlyingTag>())
                {
                    entity.RemoveComponent<FlyingTag>();
                }
                objectTypeComponent.objectType = ObjectType.Drop;
                ecb.AddComponent(entity, new MoveComponent(currentTile, currentTile, MovemetType.Forced));
                currentTile.hasCreature = false;
                currentTile.Save();

                PopUpCreator.CreatePopUp(currentTile.position, PopupType.Death);
                ecb.AddComponent(entity, new ClearOverHeadAnimations());
                if (entity.HasComponent<UniqueTag>())
                {
                    Registers.UniqueObjectsRegister.Set(iDComponent.simpleObjectName, true, true);
                }

                Debug.Log(entity.GetName() + " is dead. Responsible entity is " + killCreatureComponent.responsibleEntity.GetName());
            }
            else
            {
                Player.Die(killCreatureComponent.responsibleEntity);
            }
            SoundSystem.ScheduleSound(SoundName.Death, currentTileComponent.CurrentTile);
        }).WithoutBurst().Run();

        UpdateECB();
        var ecb1 = CreateEntityCommandBufferParallel();
        Entities.WithAll<KillCreatureComponent>().ForEach((int entityInQueryIndex, Entity entity) =>
        {
            ecb1.RemoveComponent<KillCreatureComponent>(entityInQueryIndex, entity);
        }).WithoutBurst().ScheduleParallel();
        UpdateECB();
    }
}

public struct KillCreatureComponent : IComponentData
{
    public Entity responsibleEntity;

    public KillCreatureComponent(Entity responsibleEntity)
    {
        this.responsibleEntity = responsibleEntity;
    }
}