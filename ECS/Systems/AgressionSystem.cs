using Assets.Scripts;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public partial class AgressionSystem : SystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(ComponentType.ReadWrite<FindAgressionTargetTag>(), ComponentType.ReadWrite<CreatureComponent>());
        var ecb = _manualCommanBufferSytem.CreateCommandBuffer();
        var creaturesquery = GetEntityQuery(
            ComponentType.ReadOnly<AliveTag>(),
            ComponentType.Exclude<KillCreatureComponent>(),
            ComponentType.Exclude<DestroyEntityTag>()
            );
        var creatures = creaturesquery.ToEntityArray(Allocator.TempJob);
        new FindAgressionTargetJob()
        {
            creatures = creatures,
            squadComponentFromEntity = GetComponentLookup<SquadMemberComponent>(true),
            currentTileFromEntity = GetComponentLookup<CurrentTileComponent>(true),
            tagsBufferFromEntity = GetBufferLookup<TagBufferElement>(true),
            enemyTagsBufferFromEntity = GetBufferLookup<EnemyTagBufferElement>(true),
        }.ScheduleParallel(query);
        Dependency.Complete();
        var array = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        ecb.RemoveComponent<FindAgressionTargetTag>(array);
        array.Dispose();
        _manualCommanBufferSytem.Update();
    }

    private partial struct FindAgressionTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<SquadMemberComponent> squadComponentFromEntity;
        [ReadOnly] public ComponentLookup<CurrentTileComponent> currentTileFromEntity;
        [ReadOnly] public BufferLookup<TagBufferElement> tagsBufferFromEntity;
        [ReadOnly] public BufferLookup<EnemyTagBufferElement> enemyTagsBufferFromEntity;
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<Entity> creatures;

        public void Execute([EntityIndexInQuery] int index, Entity entity, in CreatureComponent creatureComponent)
        {
            var selfSquadIndex = squadComponentFromEntity[entity].squadIndex;
            var currentTile = currentTileFromEntity[entity];
            var squareViewDistance = Mathf.Pow(StatsCalculator.CalculateViewDistance(creatureComponent, Registers.GlobalMapRegister.CurrentLocation), 2);
            var selfEnemyTags = enemyTagsBufferFromEntity[entity];
            var selfTags = tagsBufferFromEntity[entity];

            foreach (var creature in creatures)
            {
                var creatureSquadIndex = squadComponentFromEntity[creature].squadIndex;

                if (creatureSquadIndex == selfSquadIndex)//check is it squadmate
                {
                    continue;
                }

                var distance = (currentTile.currentTileId.ToMapPosition() - currentTileFromEntity[creature].currentTileId.ToMapPosition()).SqrMagnitude();
                if (distance < squareViewDistance)
                {
                    if (Registers.SquadsRegister.AreSquadsEnemies(selfSquadIndex, creatureSquadIndex)) //check is it an enemy of a squad
                    {
                        continue;
                    }
                    var entityEnemyTags = enemyTagsBufferFromEntity[creature];

                    var entityTags = tagsBufferFromEntity[creature];
                    if (selfSquadIndex != 1)
                    {
                        for (int x = 0; x < selfEnemyTags.Length; x++)
                        {
                            for (int y = 0; y < entityTags.Length; y++)
                            {
                                if (selfEnemyTags[x].tag == entityTags[y].tag)
                                {
                                    Registers.SquadsRegister.AddEnemyIndex(selfSquadIndex, creatureSquadIndex);
                                    return;
                                }
                            }
                        }

                        for (int x = 0; x < entityEnemyTags.Length; x++)
                        {
                            for (int y = 0; y < selfTags.Length; y++)
                            {
                                if (entityEnemyTags[x].tag == selfTags[y].tag)
                                {
                                    Registers.SquadsRegister.AddEnemyIndex(selfSquadIndex, creatureSquadIndex);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}