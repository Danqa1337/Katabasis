using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public enum ChangeMode
{
    Add,
    Remove,
}

[DisableAutoCreation]
public partial class EquipmentSystem : MySystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    public static event Action OnPlayerEquipChanged;

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        _debug = LowLevelSettings.instance.debugEquip;

        Entities.ForEach((Entity entity,
            ref EquipmentComponent equipmentComponent,
            ref AnatomyComponent anatomyComponent,
            ref DynamicBuffer<ChangeEquipmentElement> changeEquipmentElements,
            in CurrentTileComponent currentTileComponent
            ) =>
        {
            foreach (var element in changeEquipmentElements)
            {
                var item = element.item;
                var equipTag = element.equipTag;
                var canHold = anatomyComponent.CanHold(equipTag);
                var currentItem = equipmentComponent.GetEquipmentEntity(equipTag);

                if (!canHold && item != Entity.Null)
                {
                    throw new ArgumentOutOfRangeException("Trying to equip " + item.GetName() + " as " + equipTag + " , but can not hold It");
                }

                if (currentItem != Entity.Null && !currentItem.HasComponent<_EquipItemComponent>())
                {
                    ecb.SetComponent(currentItem, currentTileComponent);
                    ecb.AddComponent(currentItem, new _UnEquipItemComponent());
                    if (_debug) NewDebugMessage(entity.GetName() + " unequiped " + currentItem.GetName());
                }

                if (item != Entity.Null) //equipNewItem
                {
                    var offset = equipmentComponent.GetOffset(equipTag);

                    //attach item to part

                    if (equipTag == EquipTag.Weapon || equipTag == EquipTag.Shield)
                    {
                        //offset += element.item.GetComponentObject<RendererComponent>().SpriteCenterOffset;
                    }
                    if (element.item.HasComponent<MoveComponent>()) ecb.RemoveComponent<MoveComponent>(element.item);
                    if (element.item.HasComponent<ImpulseComponent>()) ecb.RemoveComponent<ImpulseComponent>(element.item);
                    ecb.AddComponent(item, new _EquipItemComponent() { parent = entity, offset = new Vector3(offset.x, offset.y, GetRendererZ(equipTag)) });
                    ecb.AddComponent(item, new Parent() { Value = entity });

                    if (_debug) NewDebugMessage(entity.GetName() + " equiped " + item.GetName() + " as " + equipTag);
                }

                switch (equipTag)
                {
                    case EquipTag.Weapon:
                        equipmentComponent.itemInMainHand = item;
                        break;

                    case EquipTag.Shield:
                        equipmentComponent.itemInOffHand = item;
                        break;

                    case EquipTag.Headwear:
                        equipmentComponent.itemOnHead = item;
                        break;

                    case EquipTag.Chestplate:
                        equipmentComponent.itemOnChest = item;
                        break;

                    default: throw new ArgumentOutOfRangeException();
                }

                if (entity.IsPlayer())
                {
                    ScheduleTriggerEvent(OnPlayerEquipChanged);
                }
            }
            ecb.RemoveComponent<ChangeEquipmentElement>(entity);
        }).WithoutBurst().Run();

        WriteDebug();
        UpdateECB();
        TriggerEvents();

        ecb = CreateEntityCommandBuffer();
        Entities.ForEach((Entity entity, Transform transform, RendererComponent rendererComponent, in _UnEquipItemComponent attachToHolderComponent, in CurrentTileComponent currentTileComponent) =>
        {
            ecb.RemoveComponent<Parent>(entity);
            ecb.RemoveComponent<_UnEquipItemComponent>(entity);
            ecb.AddComponent(entity, new MoveComponent(currentTileComponent.currentTileId, currentTileComponent.currentTileId, MovemetType.Forced));
            transform.SetParent(null);
            rendererComponent.spritesSortingLayer = "Drop";
        }).WithoutBurst().Run();

        UpdateECB();
        ecb = CreateEntityCommandBuffer();

        Entities.ForEach((Entity entity, Transform transform, RendererComponent rendererComponent, in _EquipItemComponent attachToHolderComponent, in CurrentTileComponent currentTileComponent) =>
        {
            ecb.RemoveComponent<_EquipItemComponent>(entity);
            ecb.AddComponent(entity, new Parent { Value = attachToHolderComponent.parent });
            if (entity.HasComponent<MoveComponent>()) ecb.RemoveComponent<MoveComponent>(entity);
            currentTileComponent.currentTileId.ToTileData().Remove(entity);
            transform.SetParent(attachToHolderComponent.parent.GetComponentObject<EntityAuthoring>().partsHolder);
            transform.localPosition = attachToHolderComponent.offset;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            rendererComponent.transform.localRotation = Quaternion.Euler(Vector3.zero);
            rendererComponent.spritesSortingLayer = "Solid";
            rendererComponent.transform.localPosition = Vector3.zero;
        }).WithoutBurst().Run();

        UpdateECB();
    }

    private static int GetRendererZ(EquipTag tag) => tag switch
    {
        EquipTag.Weapon => -4,
        EquipTag.Shield => -5,
        EquipTag.Headwear => -3,
        EquipTag.Chestplate => -1,
        EquipTag.None => -1
    };

    private struct _EquipItemComponent : IComponentData
    {
        public Entity parent;
        public Vector3 offset;
    }

    private struct _UnEquipItemComponent : IComponentData
    {
    }
}

public struct ChangeEquipmentElement : IBufferElementData
{
    public Entity item;
    public EquipTag equipTag;

    public ChangeEquipmentElement(Entity item, EquipTag equipTag)
    {
        this.item = item;
        this.equipTag = equipTag;
    }
}