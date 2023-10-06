using Assets.Scripts;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[DisableAutoCreation]
public partial class MovementSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        _debug = LowLevelSettings.instance.debugMovement;

        int entitiesCount = 0;
        var ecb = CreateEntityCommandBufferParallel();
        Entities.ForEach((int entityInQueryIndex, Entity entity, ref CurrentTileComponent currentTileComponent, in ObjectTypeComponent objectTypeComponent, in MoveComponent moveComponent) =>
        {
            entitiesCount++;
            if (_debug) NewDebugMessage(entity.GetName() + " moves from " + moveComponent.prevTileId.ToTileData() + " to " + moveComponent.nextTileId.ToTileData() + " as " + objectTypeComponent.objectType);

            ecb.RemoveComponent<MoveComponent>(entityInQueryIndex, entity);
            if (!entity.HasComponent<Parent>())
            {
                var objectType = objectTypeComponent.objectType;
                var currentTile = moveComponent.prevTileId.ToTileData();
                var nextTile = moveComponent.nextTileId.ToTileData();
                var animationType = AnimationType.PositionChange;
                if (!nextTile.valid)
                {
                    throw new Exception("Next tile is null. Current tile is " + currentTile);
                }

                if (objectType != ObjectType.Solid || nextTile.SolidLayer == Entity.Null || nextTile == currentTile)
                {
                    switch (moveComponent.movemetType)
                    {
                        case MovemetType.SelfPropeled:
                            if (nextTile.visible)
                            {
                                if (!entity.HasTag(Tag.Flying) && !nextTile.isAbyss && !entity.HasTag(Tag.Immaterial) && entity.GetObjectType() == ObjectType.Solid)
                                {
                                    if (nextTile.LiquidLayer != Entity.Null)
                                    {
                                        TempObjectSystem.SpawnTempObject(TempObjectType.WaterRipple, nextTile);
                                        SoundSystem.ScheduleSound(nextTile.LiquidLayer.GetComponentData<ObjectSoundData>().StepSound, nextTile);
                                    }
                                    else
                                    {
                                        foreach (var item in nextTile.GroundCoverLayer)
                                        {
                                            if (item.GetComponentData<ObjectSoundData>().StepSound != SoundName.Null)
                                            {
                                                SoundSystem.ScheduleSound(item.GetComponentData<ObjectSoundData>().StepSound, nextTile);
                                                break;
                                            }
                                        }
                                        TempObjectSystem.SpawnTempObject(TempObjectType.SmallDust, nextTile);
                                        SoundSystem.ScheduleSound(nextTile.FloorLayer.GetComponentData<ObjectSoundData>().StepSound, nextTile);
                                    }
                                }
                            }
                            animationType = AnimationType.Step;
                            break;

                        case MovemetType.Forced:
                            animationType = AnimationType.Flip;
                            if (entity.HasComponent<ProjectileTag>() && entity.HasComponent<ImpulseComponent>())
                            {
                                animationType = AnimationType.Flight;
                            }
                            break;

                        default:
                            animationType = AnimationType.PositionChange;
                            break;
                    }

                    currentTile.Remove(entity);

                    if (nextTile.isAbyss && !entity.HasComponent<FlyingTag>())
                    {
                        if (objectType == ObjectType.Drop || objectType == ObjectType.Solid)
                        {
                            if (!entity.HasComponent<ImpulseComponent>())
                            {
                                ecb.AddComponent(entityInQueryIndex, entity, new DropToAbyssTag());
                            }
                        }
                    }

                    if (nextTile == currentTile)
                    {
                        currentTile.Add(entity, objectTypeComponent.objectType);
                    }
                    else
                    {
                        nextTile.Add(entity, objectTypeComponent.objectType);

                        if (entity.HasComponent<AliveTag>())
                        {
                            currentTile.hasCreature = false;
                            nextTile.hasCreature = true;
                        }
                        if (entity.HasComponent<PlayerTag>())
                        {
                            currentTile.isPlayersTile = false;
                            nextTile.isPlayersTile = true;
                        }

                        currentTileComponent.currentTileId = nextTile.index;
                        currentTile.Save();
                        nextTile.Save();
                        if (moveComponent.movemetType == MovemetType.SelfPropeled)
                        {
                            ecb.AddOrAppendBufferElement(entityInQueryIndex, entity, new MajorAnimationElement(currentTile, nextTile, animationType));
                        }
                        else
                        {
                            ecb.AddOrAppendBufferElement(entityInQueryIndex, entity, new MinorAnimationElement(currentTile, nextTile));
                        }
                        if (entity.HasComponent<LOSBlockTag>() || entity.IsPlayer() || Registers.SquadsRegister.AreSquadmates(entity, Player.PlayerEntity))
                        {
                            FOVSystem.ScheduleFOWUpdate();
                        }
                        if (nextTile.visible)
                        {
                            nextTile.ShowUnmapable();
                        }
                    }

                    if (objectTypeComponent.objectType == ObjectType.Drop && entity.HasComponent<BodyPartComponent>() && !entity.HasComponent<Parent>() && entity.HasComponent<InternalLiquidComponent>())
                    {
                        ecb.AddComponent(entityInQueryIndex, entity, new SpillLiquidComponent(0, 1));
                    }
                }
            }
            else
            {
                throw new Exception(entity.GetName() + " is attached to " + entity.GetComponentData<Parent>().Value.GetName());
            }
        }).WithoutBurst().Run();

        UpdateECB();
        WriteDebug();
    }
}