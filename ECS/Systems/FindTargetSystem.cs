using Assets.Scripts;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public partial class FindTargetSystem : MySystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnUpdate()
    {
        _debug = LowLevelSettings.instance.debugTargetSearch;

        var searchersQuery = GetEntityQuery(
            ComponentType.ReadOnly<AliveTag>(),
            ComponentType.ReadOnly<SquadMemberComponent>(),
            ComponentType.ReadWrite<FindNewTargetTag>(),
            ComponentType.ReadWrite<AIComponent>()
            );
        if (!searchersQuery.IsEmpty)
        {
            var ecb = _manualCommanBufferSytem.CreateCommandBuffer();
            var targetsQuery = GetEntityQuery(
                ComponentType.ReadOnly<AliveTag>(),
                ComponentType.ReadOnly<AIComponent>(),
                ComponentType.ReadOnly<CreatureComponent>(),
                ComponentType.Exclude<KillCreatureComponent>(),
                ComponentType.Exclude<DestroyEntityTag>(),
                ComponentType.Exclude<DummyTag>()
                );
            var creatures = targetsQuery.ToEntityArray(Allocator.TempJob);

            var findTargetJob = new FindTargetJob()
            {
                creatures = creatures,
                squadComponentFromEntity = GetComponentLookup<SquadMemberComponent>(true),
                currentTileFromEntity = GetComponentLookup<CurrentTileComponent>(true),
            };
            var handle = findTargetJob.ScheduleParallel(searchersQuery, new JobHandle());
            handle.Complete();
            var array = searchersQuery.ToEntityArray(Allocator.Temp);

            if (LowLevelSettings.instance.debugTargetSearch)
            {
                var entities = searchersQuery.ToEntityArray(Allocator.Temp);

                for (int i = 0; i < entities.Length; i++)
                {
                    var aiComponent = entities[i].GetComponentData<AIComponent>();
                    NewDebugMessage(entities[i].GetName() + " searched for target. Result: " + aiComponent.target.GetName());
                }

                entities.Dispose();
            }
            ecb.RemoveComponent<FindNewTargetTag>(array);
            _manualCommanBufferSytem.Update();
            WriteDebug();
            array.Dispose();
        }
    }
}

public partial struct FindTargetJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<SquadMemberComponent> squadComponentFromEntity;
    [ReadOnly] public ComponentLookup<CurrentTileComponent> currentTileFromEntity;
    [DeallocateOnJobCompletion][ReadOnly] public NativeArray<Entity> creatures;

    public void Execute(Entity entity, ref AIComponent aIComponent, in CreatureComponent creatureComponent)
    {
        var selfSquadIndex = squadComponentFromEntity[entity].squadIndex;
        var currentTile = currentTileFromEntity[entity];
        var bestTarget = Entity.Null;
        var bestDistance = 999f;
        var squareViewDistance = Mathf.Pow(StatsCalculator.CalculateViewDistance(creatureComponent, Registers.GlobalMapRegister.CurrentLocation), 2);
        foreach (var creature in creatures)
        {
            var entitySquadIndex = squadComponentFromEntity[creature].squadIndex;

            if (entitySquadIndex == selfSquadIndex)//check is it squadmate
            {
                continue;
            }

            if (Registers.SquadsRegister.AreSquadsEnemies(selfSquadIndex, entitySquadIndex)) //check is it an enemy of a squad
            {
                var distance = (currentTile.currentTileId.ToMapPosition() - currentTileFromEntity[creature].currentTileId.ToMapPosition()).SqrMagnitude();

                if (distance < bestDistance && distance < squareViewDistance)
                {
                    bestTarget = creature;
                    bestDistance = distance;
                }
            }
        }

        if (bestTarget != Entity.Null)
        {
            aIComponent.target = bestTarget;
        }
    }
}

//    public struct FindTargetJobOld : IJob
//{
//    public AiBehviourJobDataOld data;

//    public void Execute()
//    {
//        var creatureComponent = data.creatureComponentFromEntity[data.self];
//        var currenttile = data.currentTile;
//        data.tilesInViewDistance.CopyFrom(data.tilesInViewDistance.ToArray().OrderBy(t => t.GetSqrDistance(currenttile)).ToArray());

//        #region SearchForItemToPicup

//        if (data.abilityBufferfromEntity[data.self].Contains(new AvaibleAbilitiesElement(Ability.PicUp)))
//        {
//            var bestItem = data.ItemInMainHand;
//            var besttDpt = 0f;
//            var bestTile = TileData.Null;
//            if (bestItem != Entity.Null)
//            {
//                besttDpt = DamageCalculator.CalculateDPT(creatureComponent, data.physicsComponentFromEntity[data.ItemInMainHand]);
//            }

//            foreach (var tile in data.currentTile.GetNeibors(true)) //find best item
//            {
//                foreach (var item in data.dropBufferFromEntity[tile.tileEntity])
//                {
//                    if (item.entity != Entity.Null && !data.impulseComponentFromEntity.HasComponent(item.entity))
//                    {
//                        if (!data.physicsComponentFromEntity.HasComponent(item.entity))
//                        {
//                            Debug.Log(item + " " + tile);
//                        }
//                        var currentDPT = DamageCalculator.CalculateDPT(creatureComponent, data.physicsComponentFromEntity[item.entity]);
//                        if (false && currentDPT > besttDpt)
//                        {
//                            besttDpt = currentDPT;
//                            bestItem = item.entity;
//                            bestTile = tile;
//                        }
//                    }
//                }

//            }
//            if (bestItem != data.ItemInMainHand)
//            {
//                data.targetSearchResult.Value = bestItem;
//                data.targetTile.Value = bestTile;
//                data.targetType.Value = ObjectType.Drop;
//                return;
//            }
//        }

//        #endregion

//        #region SearchForEnemy

//        var selfTags = data.tagsBufferFromEntity[data.self];
//        var selfEnemyTags = data.enemyTagsBufferFromEntity[data.self];
//        var selfSquadIndex = data.squadComponentFromEntity[data.self].squadIndex;

//        foreach (var tile in data.tilesInViewDistance)
//        {
//            if (tile.SolidLayer != Entity.Null && tile.SolidLayer != data.self)
//            {
//                if (tile.hasCreature)
//                {
//                    var entityEnemyTags = data.enemyTagsBufferFromEntity[tile.SolidLayer];

//                    var entitySquadIndex = data.squadComponentFromEntity[tile.SolidLayer].squadIndex;
//                    if (entitySquadIndex == selfSquadIndex)//check is it squadmate
//                    {
//                        continue;
//                    }
//                    else
//                    {
//                        if (SquadsRegister.AreSquadsEnemies(selfSquadIndex, entitySquadIndex))
//                        {
//                            data.targetSearchResult.Value = tile.SolidLayer;
//                            data.targetTile.Value = tile;
//                            data.targetType.Value = ObjectType.Solid;
//                            return;
//                        }
//                    }

//                    var entityTags = data.tagsBufferFromEntity[tile.SolidLayer];
//                    if (selfSquadIndex != 1)
//                    {
//                        for (int x = 0; x < selfEnemyTags.Length; x++)
//                        {
//                            for (int y = 0; y < entityTags.Length; y++)
//                            {
//                                if (selfEnemyTags[x].tag == entityTags[y].tag)
//                                {
//                                    data.targetSearchResult.Value = tile.SolidLayer;
//                                    data.targetTile.Value = tile;
//                                    data.targetType.Value = ObjectType.Solid;

//                                    return;
//                                }
//                            }
//                        }

//                        for (int x = 0; x < entityEnemyTags.Length; x++)
//                        {
//                            for (int y = 0; y < selfTags.Length; y++)
//                            {
//                                if (entityEnemyTags[x].tag == selfTags[y].tag)
//                                {
//                                    data.targetSearchResult.Value = tile.SolidLayer;
//                                    data.targetTile.Value = tile;
//                                    data.targetType.Value = ObjectType.Solid;

//                                    return;
//                                }
//                            }
//                        }
//                    }
//                }

//            }

//        }

//        #endregion

//    }
//}
[BurstCompile]
public struct GetTilesInRadiusJob : IJob
{
    [ReadOnly] public TileData center;
    [ReadOnly] public int radius;
    [ReadOnly] public NativeArray<TileData> map;
    [WriteOnly] public NativeList<TileData> result;

    public void Execute()
    {
        BaseMethodsClass.GetTilesInRadiusNative(center, radius, map, result);
    }
}