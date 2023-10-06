using Assets.Scripts;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public static class EntitySerializer
{
    public static SimpleObjectData SerializeAsSimpleObject(Entity entity, EntityManager entityManager)
    {
        if (entity != Entity.Null)
        {
            if (entity.Index > entityManager.EntityCapacity) Debug.Log(entity.Index + " > " + entityManager.EntityCapacity);
            if (!entity.HasComponent<SimpleObjectNameComponent>(entityManager)) return null;
            if (!entity.HasComponent<CurrentTileComponent>(entityManager)) return null;

            var simpleObjectName = entity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName;
            var data = SimpleObjectsDatabase.GetObjectData(simpleObjectName);

            data.currentTileComponent = entity.GetComponentData<CurrentTileComponent>();
            data.objectTypeComponent = entity.GetComponentData<ObjectTypeComponent>();

            if (entity.HasComponent<RendererComponent>(entityManager))
            {
                var renderer = entity.GetComponentObject<RendererComponent>(entityManager);
                data.rndSpriteNum = renderer.SpriteIndex;
                data.altSpriteDrown = renderer.AltSpriteDrown;
            }

            data.durabilityComponent = GetComponentReferece<DurabilityComponent>(entity, entityManager);
            data.decayComponent = GetComponentReferece<DecayComponent>(entity, entityManager);
            data.physicsComponent = GetComponentReferece<PhysicsComponent>(entity, entityManager);
            data.rangedWeaponComponent = GetComponentReferece<RangedWeaponComponent>(entity, entityManager);
            data.stairsComponent = GetComponentReferece<StairsComponent>(entity, entityManager);
            data.doorComponent = GetComponentReferece<DoorComponent>(entity, entityManager);
            data.lockComponent = GetComponentReferece<LockComponent>(entity, entityManager);
            data.keyComponent = GetComponentReferece<KeyComponent>(entity, entityManager);

            data.tagElements = GetBuffer<TagBufferElement>(entity, entityManager);
            data.activeEffects = GetBuffer<EffectElement>(entity, entityManager);

            return data;
        }
        return null;
    }

    public static ComplexObjectData SerializeAsComplexObject(Entity entity, EntityManager entityManager)
    {
        if (entity.Index > entityManager.EntityCapacity) Debug.Log(entity.Index + " > " + entityManager.EntityCapacity);
        if (entity == Entity.Null) throw new System.NullReferenceException("entity is null");
        if (!entity.HasComponent<SimpleObjectNameComponent>(entityManager)) return null;
        if (!entity.HasComponent<CurrentTileComponent>(entityManager)) return null;

        var complexObjectName = entity.GetComponentData<ComplexObjectNameComponent>().complexObjectName;
        var data = ComplexObjectsDatabase.GetObjectData(complexObjectName);

        data.squadMemberComponent = GetComponentReferece<SquadMemberComponent>(entity, entityManager);
        data.moraleComponent = GetComponentReferece<MoraleComponent>(entity, entityManager);
        data.creatureComponent = GetComponentReferece<CreatureComponent>(entity, entityManager);

        data.availableAbilities = GetBuffer<AbilityElement>(entity, entityManager);
        data.enemyTagElements = GetBuffer<EnemyTagBufferElement>(entity, entityManager);

        data.alive = entity.HasComponent<AliveTag>(entityManager);

        if (entity.HasComponent<InventoryComponent>(entityManager))
        {
            foreach (var item in entity.GetBuffer<InventoryBufferElement>(entityManager))
            {
                data.itemsInInventory.Add(SerializeAsSimpleObject(item.entity, entityManager));
            }
        }
        else
        {
            data.itemsInInventory = null;
        }

        if (entity.HasComponent<EquipmentComponent>(entityManager))
        {
            var component = entity.GetComponentData<EquipmentComponent>(entityManager);
            data.itemInMainHand = SerializeAsSimpleObject(component.itemInMainHand, entityManager);
            data.itemInOffHand = SerializeAsSimpleObject(component.itemInOffHand, entityManager);
            data.itemOnChest = SerializeAsSimpleObject(component.itemOnChest, entityManager);
            data.itemOnHead = SerializeAsSimpleObject(component.itemOnHead, entityManager);
        }

        if (entity.HasComponent<AnatomyComponent>(entityManager))
        {
            var component = entity.GetComponentData<AnatomyComponent>(entityManager);
            foreach (var part in entity.GetBuffer<MissingBodypartBufferElement>(entityManager))
            {
                data.missingBodyparts.Add(part);
            }

            data.Body = SerializeAsSimpleObject(component.Body, entityManager);
            data.Head = SerializeAsSimpleObject(component.Head, entityManager);
            data.LowerBody = SerializeAsSimpleObject(component.LowerBody, entityManager);
            data.RightFrontLeg = SerializeAsSimpleObject(component.RightFrontLeg, entityManager);
            data.LeftFrontLeg = SerializeAsSimpleObject(component.LeftFrontLeg, entityManager);
            data.RightRearLeg = SerializeAsSimpleObject(component.RightRearLeg, entityManager);
            data.LeftRearLeg = SerializeAsSimpleObject(component.LeftRearLeg, entityManager);
            data.RightArm = SerializeAsSimpleObject(component.RightArm, entityManager);
            data.LeftArm = SerializeAsSimpleObject(component.LeftArm, entityManager);
            data.RightClaw = SerializeAsSimpleObject(component.RightClaw, entityManager);
            data.LeftClaw = SerializeAsSimpleObject(component.LeftClaw, entityManager);
            data.Tentacle0 = SerializeAsSimpleObject(component.Tentacle0, entityManager);
            data.Tentacle1 = SerializeAsSimpleObject(component.Tentacle1, entityManager);
            data.Tentacle2 = SerializeAsSimpleObject(component.Tentacle2, entityManager);
            data.Tentacle3 = SerializeAsSimpleObject(component.Tentacle3, entityManager);
            data.Tentacle4 = SerializeAsSimpleObject(component.Tentacle4, entityManager);
            data.Tail = SerializeAsSimpleObject(component.Tail, entityManager);
        }
        return data;
    }

    private static ComponentReferece<T> GetComponentReferece<T>(Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
    {
        if (entity.HasComponent<T>(entityManager))
        {
            return new ComponentReferece<T>(entity.GetComponentData<T>(entityManager));
        }
        return new ComponentReferece<T>();
    }

    private static List<T> GetBuffer<T>(Entity entity, EntityManager entityManager) where T : unmanaged, IBufferElementData
    {
        if (entity.HasBuffer<T>(entityManager))
        {
            var list = new List<T>();
            foreach (var item in entity.GetBuffer<T>(entityManager))
            {
                list.Add(item);
            }
            return list;
        }
        return null;
    }
}