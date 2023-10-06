using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public partial class CollisionSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        _debug = LowLevelSettings.instance.debugCollisions;
        var ecb = CreateEntityCommandBufferParallel();
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, uint.MaxValue));
        var physicsComponentFromEntity = GetComponentLookup<PhysicsComponent>(true);
        Entities.ForEach((int entityInQueryIndex, Entity entity, ref DynamicBuffer<CollisionElement> collisionElements, in AnatomyComponent anatomyComponent, in EquipmentComponent equipmentComponent, in CurrentTileComponent currentTileComponent) =>
        {
            foreach (var collisionElement in collisionElements)
            {
                var firtHitPart = entity;
                var secodHitPart = Entity.Null;
                var parts = new List<Entity>() { entity };
                var iterator = 0f;

                parts.AddRange(anatomyComponent.GetBodyPartsNotNull());
                parts.AddRange(equipmentComponent.GetEquipmentNotNull());

                var scores = new float[parts.Count];
                for (int i = 0; i < parts.Count; i++)
                {
                    var physics = parts[i].GetComponentData<PhysicsComponent>();
                    var partTag = parts[i].HasComponent<BodyPartComponent>() ? parts[i].GetComponentData<BodyPartComponent>().bodyPartTag : BodyPartTag.Body;
                    var chanceMultiplerFromPart = StatsCalculator.GetHitChanceMultiplerFromBodypartType(partTag);
                    var chanceMultiplerFromAim = GetHitChanceFromAim(collisionElement, partTag);
                    var chanceBonusFromSize = StatsCalculator.GetHitChanceMultiplerFromSize(physics.size);
                    var chance = chanceBonusFromSize * chanceMultiplerFromPart * chanceMultiplerFromAim;
                    scores[i] = chance;
                }
                var randomValue = random.NextFloat(0, scores.Sum());
                for (int i = 0; i < parts.Count; i++)
                {
                    iterator += scores[i];
                    if (randomValue < iterator)
                    {
                        firtHitPart = parts[i];
                        break;
                    }
                }

                var hitPartEquipTag = equipmentComponent.GetEquipTag(firtHitPart);

                if (hitPartEquipTag != EquipTag.None)
                {
                    switch (hitPartEquipTag)
                    {
                        case EquipTag.Weapon:
                            secodHitPart = anatomyComponent.GetBodyPart(BodyPartTag.RightArm);
                            break;

                        case EquipTag.Shield:
                            secodHitPart = anatomyComponent.GetBodyPart(BodyPartTag.LeftArm);
                            break;

                        case EquipTag.Headwear:
                            secodHitPart = anatomyComponent.GetBodyPart(BodyPartTag.Head);
                            break;

                        case EquipTag.Chestplate:
                            secodHitPart = entity;
                            break;

                        case EquipTag.None:
                            secodHitPart = Entity.Null;
                            break;
                    }
                }

                if (firtHitPart != entity)
                {
                    ecb.AddBufferElement(entityInQueryIndex, firtHitPart, collisionElement);
                    if (collisionElements.Length == 1)
                    {
                        collisionElements.Remove(collisionElement);
                    }
                    else
                    {
                        ecb.RemoveComponent<CollisionElement>(entityInQueryIndex, entity);
                    }
                }

                if (secodHitPart != Entity.Null)
                {
                    if (secodHitPart == entity)
                    {
                        collisionElements.Remove(collisionElement);
                    }
                    var resistance = random.NextInt(0, (int)physicsComponentFromEntity[firtHitPart].resistance);
                    PopUpCreator.CreatePopUp(currentTileComponent.CurrentTile.position, resistance.ToString() + "#", Color.gray);
                    ecb.AddBufferElement(entityInQueryIndex, secodHitPart, new CollisionElement(collisionElement.bonusDamage - resistance, collisionElement.collidedEntity, collisionElement.responsibleEntity));
                }
            }
        }).WithoutBurst().Run();

        UpdateECB();
        ecb = CreateEntityCommandBufferParallel();

        physicsComponentFromEntity = GetComponentLookup<PhysicsComponent>(true);

        Entities.ForEach((int entityInQueryIndex, Entity entity, in DynamicBuffer<CollisionElement> collisionElements, in PhysicsComponent physicsComponent, in DurabilityComponent durabilityComponent, in ObjectTypeComponent objectTypeComponent, in CurrentTileComponent currentTileComponent) =>
        {
            ecb.RemoveComponent<CollisionElement>(entityInQueryIndex, entity);
            foreach (var collisionElement in collisionElements)
            {
                var initialDamage = collisionElement.bonusDamage + physicsComponentFromEntity[collisionElement.collidedEntity].damage;
                var resistance = random.NextInt(0, (int)physicsComponent.resistance);
                var playerDamageMultipler = entity.HasComponent<PlayerTag>() || (entity.HasComponent<Parent>() && entity.GetComponentData<Parent>().Value.HasComponent<PlayerTag>()) ? PlayerSettings.instance.incomingDamageMultipler : 1f;
                var finalDamage = (int)Mathf.Max(0, (initialDamage - resistance) * playerDamageMultipler);

                if (entity.HasComponent<ExtraFragileTag>() || entity.HasComponent<ExplosiveTag>())
                {
                    ecb.AddComponent(entityInQueryIndex, entity, new BreakObjectComponent());
                }
                else
                {
                    if (durabilityComponent.currentDurability - finalDamage > 0)
                    {
                        ecb.AddOrAppendBufferElement(entityInQueryIndex, entity, new DurabilityChangeElement(-finalDamage, collisionElement.responsibleEntity, DurabilityChangeReason.Smashed));
                        if (!entity.HasComponent<Parent>() && objectTypeComponent.objectType == ObjectType.Drop)
                        {
                            ecb.AddComponent(entityInQueryIndex, entity, new MoveComponent(currentTileComponent.CurrentTile, currentTileComponent.CurrentTile, MovemetType.Forced));
                        }
                    }
                    else
                    {
                        if (entity.HasComponent<Parent>() && entity.HasComponent<BodyPartComponent>())
                        {
                            ecb.AddOrAppendBufferElement(entityInQueryIndex, entity.GetComponentData<Parent>().Value, new AnatomyChangeElement(Entity.Null, entity.GetComponentData<BodyPartComponent>().bodyPartTag, collisionElement.responsibleEntity));
                            ecb.AddComponent(entityInQueryIndex, entity, new ImpulseComponent(random.NextFloat2Direction(), 0, 3, collisionElement.responsibleEntity));
                        }
                        else
                        {
                            ecb.AddOrAppendBufferElement(entityInQueryIndex, entity, new DurabilityChangeElement(-finalDamage, collisionElement.responsibleEntity, DurabilityChangeReason.Smashed));
                        }
                    }
                }
                ecb.AddComponent(entityInQueryIndex, entity, new DrawHitAnimationTag());
                if (_debug) NewDebugMessage(collisionElement.collidedEntity.GetName() + " collides with " + entity.GetName() + ". Final damage: " + finalDamage + ". Resistance: " + resistance);
            }
        }).WithoutBurst().Run();
        UpdateECB();
        WriteDebug();
    }

    private static float GetHitChanceFromAim(CollisionElement collisionElement, BodyPartTag bodyPartTag)
    {
        return 1;
    }
}

public struct CollisionElement : IBufferElementData
{
    public Entity collidedEntity;
    public Entity responsibleEntity;
    public int bonusDamage;

    public CollisionElement(Entity collidedEntity, Entity responsibleEntity, int bonusDamage)
    {
        this.collidedEntity = collidedEntity;
        this.responsibleEntity = responsibleEntity;
        this.bonusDamage = bonusDamage;
    }

    public CollisionElement(int bonusDamage, Entity collidedEntity, Entity responsibleEntity)
    {
        this.collidedEntity = collidedEntity;
        this.responsibleEntity = responsibleEntity;
        this.bonusDamage = bonusDamage;
    }
}

public struct AimChanceMultipler
{
    public BodyPartTag bodyPart;
    public float multipler;

    public AimChanceMultipler(BodyPartTag bodyPart, float bonus)
    {
        this.bodyPart = bodyPart;
        this.multipler = bonus;
    }
}