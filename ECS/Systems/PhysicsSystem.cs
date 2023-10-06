using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
public partial class PhysicsSystem : MySystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();
        var debug = LowLevelSettings.instance.debugPhysics;

        var query = EntityManager.CreateEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ImpulseComponent), typeof(CurrentTileComponent) },
            None = new ComponentType[] { typeof(UnmovableTag), typeof(DestroyEntityTag) }
        });
        var parallelDebugMessages = new UnsafeParallelHashSet<FixedString512Bytes>(query.CalculateEntityCount(), Allocator.TempJob);

        Entities.ForEach((int entityInQueryIndex, Entity entity, ref ImpulseComponent impulseComponent, in CurrentTileComponent currentTileComponent) =>
        {
            var currentTile = currentTileComponent.currentTileId.ToTileData();

            var debugmessage = new FixedString512Bytes();

            if (debug) debugmessage = entity.GetName() + " is affected by impulse ";

            if (!entity.HasComponent<UnmovableTag>())
            {
                if (impulseComponent.H > 0 && !impulseComponent.axelerationVector.Equals(float2.zero))
                {
                    var nextTile = impulseComponent.calculateNextTile(currentTile, out impulseComponent.err);
                    if (nextTile.SolidLayer != Entity.Null && !nextTile.SolidLayer.Exists()) throw new System.Exception("next solid does not exists");

                    if (nextTile.SolidLayer != Entity.Null) // collision
                    {
                        var obstacle = nextTile.SolidLayer;

                        var collisionOnEntity = new CollisionElement(impulseComponent.bonusDamage, obstacle, impulseComponent.responsibleEntity);
                        var collisionOnObstacle = new CollisionElement(impulseComponent.bonusDamage, entity, impulseComponent.responsibleEntity);

                        ecb.AddOrAppendBufferElement(entityInQueryIndex, entity, collisionOnEntity);
                        ecb.AddOrAppendBufferElement(entityInQueryIndex, obstacle, collisionOnObstacle);
                        ecb.RemoveComponent<ImpulseComponent>(entityInQueryIndex, entity);

                        if (debug) debugmessage += "and going to collide with " + nextTile.SolidLayer.GetName();

                        if (!entity.HasComponent<Parent>())
                        {
                            ecb.AddComponent(entityInQueryIndex, entity, new MoveComponent(currentTile, currentTile, MovemetType.Forced));
                        }
                    }
                    else // no collision
                    {
                        if (!entity.HasComponent<Parent>()) //free
                        {
                            impulseComponent.H--;

                            var moveComponent = new MoveComponent(currentTile, nextTile, MovemetType.Forced);
                            ecb.AddComponent(entityInQueryIndex, entity, moveComponent);
                            if (debug) debugmessage += "and going to move from " + currentTile + " to " + nextTile;
                        }
                        else //is attached
                        {
                            if (debug) debugmessage += "but stops becouse is attached";

                            ecb.RemoveComponent<ImpulseComponent>(entityInQueryIndex, entity);
                        }
                    }
                }
                else
                {
                    if (!entity.HasComponent<Parent>())
                    {
                        if (!currentTile.isAbyss)
                        {
                            if (debug) debugmessage += "and going to fall on the ground";

                            var ground = ecb.CreateEntity(entityInQueryIndex);
                            ecb.SetName(entityInQueryIndex, ground, "Ground");
                            ecb.AddComponent(entityInQueryIndex, ground, new PhysicsComponent() { damage = 1 });
                            ecb.AddComponent(entityInQueryIndex, ground, new GroundTag());

                            var collisionWithGround = new CollisionElement(impulseComponent.bonusDamage, ground, impulseComponent.responsibleEntity);
                            ecb.AddOrAppendBufferElement(entityInQueryIndex, entity, collisionWithGround);
                        }
                        else 
                        {
                            if (debug) debugmessage += "and going to fall staight into abyss";
                        }
                        ecb.AddComponent(entityInQueryIndex, entity, new MoveComponent(currentTile, currentTile, MovemetType.Forced));
                    }
                    else
                    {
                        if (debug) debugmessage += "but stops becouse is attached";
                    }
                    ecb.RemoveComponent<ImpulseComponent>(entityInQueryIndex, entity);

                }
                parallelDebugMessages.Add(debugmessage);
            }
            else
            {
                if (debug) debugmessage += "but it is unmovable";

                ecb.RemoveComponent<ImpulseComponent>(entityInQueryIndex, entity);
            }
        }).WithoutBurst().ScheduleParallel();
        UpdateECB();

        WriteDebug(parallelDebugMessages);
    }
}