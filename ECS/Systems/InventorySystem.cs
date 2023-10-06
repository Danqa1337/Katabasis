using System;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class InventorySystem : MySystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    public static event Action OnPlayerInventoryChanged;

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnUpdate()
    {
        _debug = LowLevelSettings.instance.debugInventory;
        var ecb = CreateEntityCommandBuffer();

        Entities.ForEach((Entity entity,
            Transform transform,
            RendererComponent rendererComponent,
            ref DynamicBuffer<ChangeInventoryElement> changeInventoryElements,
            ref DynamicBuffer<InventoryBufferElement> inventoryElements,
            ref InventoryComponent inventoryComponent,
            in CurrentTileComponent currentTileComponent) =>
        {
            foreach (var element in changeInventoryElements)
            {
                if (element.add)
                {
                    if (element.item == Entity.Null)
                    {
                        throw new Exception("trying to add null to inventory of " + entity.GetName());
                    }
                    if (!element.item.Exists())
                    {
                        throw new Exception("Trying to add entity that does not exist to inventory of " + entity.GetName());
                    }
                    if (inventoryElements.Contains(new InventoryBufferElement(element.item)))
                    {
                        throw new Exception("Trying to add " + element.item.GetName() + " that is allready in inventory of " + entity.GetName());
                    }

                    inventoryElements.Add(new InventoryBufferElement(element.item));

                    if (element.item.HasComponent<MoveComponent>()) ecb.RemoveComponent<MoveComponent>(element.item);
                    if (element.item.HasComponent<ImpulseComponent>()) ecb.RemoveComponent<ImpulseComponent>(element.item);
                    ecb.AddComponent(element.item, new AttachToInventoryComponent(entity));
                    if (_debug) NewDebugMessage(element.item.GetName() + " added to " + entity.GetName() + "'s inventory");
                }
                else
                {
                    if (element.item == Entity.Null)
                    {
                        throw new Exception("trying to remove null from inventory");
                    }
                    if (!element.item.Exists())
                    {
                        throw new Exception("Trying to remove entity that does not exist");
                    }
                    if (!inventoryElements.Contains(new InventoryBufferElement(element.item)))
                    {
                        throw new Exception("Trying to remove item that in not present in inventory");
                    }

                    inventoryElements.Remove(new InventoryBufferElement(element.item));

                    ecb.AddComponent(element.item, new DetachFromInventoryComponent(entity));
                    ecb.SetComponent(element.item, currentTileComponent);

                    if (_debug) NewDebugMessage(element.item.GetName() + " removed from " + entity.GetName() + "'s inventory");
                }
            }
            if (entity.IsPlayer())
            {
                ScheduleTriggerEvent(OnPlayerInventoryChanged);
            }

            ecb.RemoveComponent<ChangeInventoryElement>(entity);
        }).WithoutBurst().Run();
        UpdateECB();

        ecb = CreateEntityCommandBuffer();
        Entities.ForEach((Entity entity, Transform transform, RendererComponent rendererComponent, in CurrentTileComponent currentTileComponent, in AttachToInventoryComponent attachToInventoryComponent) =>
        {
            transform.SetParent(attachToInventoryComponent.parent.GetComponentObject<EntityAuthoring>().partsHolder);
            rendererComponent.spritesSortingLayer = "Default";
            currentTileComponent.currentTileId.ToTileData().Remove(entity);
            ecb.RemoveComponent<AttachToInventoryComponent>(entity);
        }).WithoutBurst().Run();

        Entities.ForEach((Entity entity, Transform transform, RendererComponent rendererComponent, in CurrentTileComponent currentTileComponent, in DetachFromInventoryComponent detachFromInventoryComponent) =>
        {
            transform.SetParent(null);
            rendererComponent.spritesSortingLayer = "Drop";
            ecb.AddComponent(entity, new MoveComponent(currentTileComponent.currentTileId.ToTileData(), currentTileComponent.currentTileId.ToTileData(), MovemetType.Forced));
            ecb.RemoveComponent<DetachFromInventoryComponent>(entity);
        }).WithoutBurst().Run();
        WriteDebug();
        UpdateECB();
        TriggerEvents();
    }

    public struct AttachToInventoryComponent : IComponentData
    {
        public Entity parent;

        public AttachToInventoryComponent(Entity parent)
        {
            this.parent = parent;
        }
    }

    public struct DetachFromInventoryComponent : IComponentData
    {
        public Entity parent;

        public DetachFromInventoryComponent(Entity parent)
        {
            this.parent = parent;
        }
    }
}

public struct ChangeInventoryElement : IBufferElementData
{
    public Entity item;
    public bool add;

    public ChangeInventoryElement(Entity item, bool add)
    {
        this.item = item;
        this.add = add;
    }
}