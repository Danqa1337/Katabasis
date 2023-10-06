using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class ExchangeSystem : MySystemBase
{
    private static List<ExchangeOffer> _exchangeOffers = new List<ExchangeOffer>();

    public static void AddOffer(ExchangeOffer exchangeOffer)
    {
        _exchangeOffers.Add(exchangeOffer);
    }

    protected override void OnUpdate()
    {
        foreach (var exchangeOffer in _exchangeOffers)
        {
            var sender = exchangeOffer.sender;
            var receiver = exchangeOffer.receiver;
            var entityToReceive = exchangeOffer.entityToReceive;
            var entityToGiveBack = exchangeOffer.entityToGiveBack;

            if (sender == Entity.Null)
            {
                throw new Exception("sender is null, receiver is " + receiver.GetName());
            }

            if (receiver == Entity.Null)
            {
                throw new Exception("receiver is null, sender is " + sender.GetName());
            }

            if (!sender.Exists() || !receiver.Exists())
            {
                continue;
            }

            if ((entityToReceive != Entity.Null && !entityToReceive.Exists()) || (entityToGiveBack != Entity.Null && !entityToGiveBack.Exists()))
            {
                continue;
            }

            if (receiver.HasComponent<TraderComponent>() && receiver.HasBuffer<TradingItemElement>())
            {
                if (entityToReceive.GetComponentData<SimpleObjectNameComponent>().simpleObjectName == SimpleObjectName.MediumGoldPiece)
                {
                    Accept();

                    var tradingItemsBuffer = receiver.GetBuffer<TradingItemElement>();
                    for (int i = 0; i < tradingItemsBuffer.Length; i++)
                    {
                        if (tradingItemsBuffer[i].item == entityToGiveBack)
                        {
                            tradingItemsBuffer.Remove(tradingItemsBuffer[i]);
                            break;
                        }
                    }
                    if (tradingItemsBuffer.Length > 0)
                    {
                        receiver.AddBufferElement(new ChangeEquipmentElement(tradingItemsBuffer[UnityEngine.Random.Range(0, tradingItemsBuffer.Length - 1)].item, EquipTag.Weapon));
                    }
                    else
                    {
                        receiver.RemoveBuffer<TradingItemElement>();
                        receiver.AddComponentData(new AbilityComponent(AbilityName.TeleportAway));
                    }
                    receiver.AddBufferElement(new ChangeInventoryElement(entityToReceive, true));
                }
                else
                {
                    Decline();
                }
            }

            if (receiver.HasComponent<SlaverComponent>())
            {
                var slaves = receiver.GetComponentData<SlaverComponent>().GetSlaves();
                if (entityToReceive.GetComponentData<SimpleObjectNameComponent>().simpleObjectName == SimpleObjectName.MediumGoldPiece && slaves.Count > 0 && Registers.SquadsRegister.GetPlayersSquad().members.Count <= 5)
                {
                    Accept();
                    receiver.AddBufferElement(new ChangeInventoryElement(entityToReceive, true));
                    var slave = slaves.RandomItem();
                    var slaveLocked = true;
                    Registers.SquadsRegister.AddToPlayersSquad(slave);
                    slave.RemoveZeroSizedComponent<SlaveTag>();
                    foreach (var item in slave.CurrentTile().GetNeibors(true))
                    {
                        if (item.SolidLayer == Entity.Null)
                        {
                            slaveLocked = false;
                        }
                    }

                    if (slaveLocked)
                    {
                        foreach (var item in slave.CurrentTile().GetNeibors(true))
                        {
                            if (item.SolidLayer != Entity.Null)
                            {
                                if (item.SolidLayer.HasComponent<DoorComponent>() && item.SolidLayer.HasComponent<LockComponent>())
                                {
                                    var keyIndex = item.SolidLayer.GetComponentData<LockComponent>().lockIndex;
                                    var key = SimpleObjectsDatabase.GetObjectData(SimpleObjectName.Key);
                                    key.keyComponent = new ComponentReferece<KeyComponent>(new KeyComponent(keyIndex));
                                    receiver.CurrentTile().Spawn(key);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Decline();
                }
            }

            if (receiver.IsPlayersSquadmate())
            {
                if (entityToReceive != Entity.Null)
                {
                    Accept();
                    var equipTag = entityToReceive.GetComponentData<PhysicsComponent>().defaultEquipTag;
                    receiver.AddBufferElement(new ChangeEquipmentElement(entityToReceive, equipTag));
                }
                else
                {
                    Decline();
                }
            }

            void Decline()
            {
                Debug.Log(exchangeOffer.ToString() + " was declined");
                PopUpCreator.CreatePopUp(receiver.CurrentTile().position, PopupType.No);
                SoundSystem.PlaySound(SoundName.Decline);
            }

            void Accept()
            {
                if (entityToReceive != Entity.Null)
                {
                    if (sender.GetComponentData<EquipmentComponent>().GetEquipmentNotNull().Contains(entityToReceive))
                    {
                        var equipTag = sender.GetComponentData<EquipmentComponent>().GetEquipTag(entityToReceive);

                        sender.AddBufferElement(new ChangeEquipmentElement(Entity.Null, equipTag));
                    }
                    else if (sender.GetComponentData<InventoryComponent>().items.Contains(entityToReceive))
                    {
                        sender.AddBufferElement(new ChangeInventoryElement(entityToReceive, false));
                    }
                    else
                    {
                        Decline();
                        return;
                    }
                }

                if (entityToGiveBack != Entity.Null)
                {
                    if (receiver.GetComponentData<EquipmentComponent>().GetEquipmentNotNull().Contains(entityToGiveBack))
                    {
                        var equipTag = receiver.GetComponentData<EquipmentComponent>().GetEquipTag(entityToGiveBack);
                        receiver.AddBufferElement(new ChangeEquipmentElement(Entity.Null, equipTag));
                    }
                    else if (receiver.GetComponentData<InventoryComponent>().items.Contains(entityToGiveBack))
                    {
                        receiver.AddBufferElement(new ChangeInventoryElement(entityToGiveBack, false));
                    }
                    else
                    {
                        Decline();
                        return;
                    }
                }

                Debug.Log(exchangeOffer.ToString() + " was accepted");
                PopUpCreator.CreatePopUp(receiver.CurrentTile().position, PopupType.Yes);
                SoundSystem.PlaySound(SoundName.Eager);
            }
        }
        _exchangeOffers.Clear();
    }
}

public struct ExchangeOffer
{
    public Entity sender;
    public Entity receiver;
    public Entity entityToReceive;
    public Entity entityToGiveBack;

    public ExchangeOffer(Entity sender, Entity receiver, Entity entityToReceive, Entity entityToGiveBack)
    {
        this.sender = sender;
        this.receiver = receiver;
        this.entityToReceive = entityToReceive;
        this.entityToGiveBack = entityToGiveBack;
    }

    public override string ToString()
    {
        return "Exchange offer from " + sender.GetName() + " to " + receiver.GetName() + ", exchange " + entityToGiveBack.GetName() + " for " + entityToReceive.GetName();
    }
}