using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[DisableAutoCreation]
public partial class GetEvaluationDataSystem : SystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(ComponentType.ReadWrite<EvaluateBehaviourTag>());
        if (!query.IsEmpty)
        {
            var ecb = _manualCommanBufferSytem.CreateCommandBuffer().AsParallelWriter();
            var getEvaluationDataJob = new GetEvaluationDataJob()
            {
                ecb = ecb,
                abilityBuffer = GetBufferLookup<AbilityElement>(),
                currentTileFromEntity = GetComponentLookup<CurrentTileComponent>(),
                rangedWeaponFromEntity = GetComponentLookup<RangedWeaponComponent>(),
                idComponentFromEntity = GetComponentLookup<SimpleObjectNameComponent>(),
                polearmTagFromEntity = GetComponentLookup<PolearmTag>(),
                moraleComponentFromEntity = GetComponentLookup<MoraleComponent>(),
                random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 1000000)),
            };
            getEvaluationDataJob.ScheduleParallel();
            Dependency.Complete();
            _manualCommanBufferSytem.Update();
        }
    }
}

[DisableAutoCreation]
public partial class EvaluateBehaviourSystem : MySystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    protected override void OnUpdate()
    {
        var debug = LowLevelSettings.instance.debugAI;
        var query = GetEntityQuery(
            ComponentType.ReadWrite<EvaluationInputData>(),
            ComponentType.ReadWrite < SquadMemberComponent>(),
            ComponentType.ReadWrite < EquipmentComponent>() ,
            ComponentType.ReadWrite < AIComponent>() ,
            ComponentType.ReadWrite < CreatureComponent>() ,
            ComponentType.ReadWrite < InventoryComponent>() ,
            ComponentType.ReadWrite < WalkabilityDataComponent>() ,
            ComponentType.ReadWrite < CurrentTileComponent>() ,
            ComponentType.ReadWrite < InventoryBufferElement >()
            );
        if (!query.IsEmpty)
        {
            var ecb = CreateEntityCommandBufferParallel();

            var evaluateJob = new EvaluateBehaviourJob()
            {
                ecb = ecb,
            };
            evaluateJob.ScheduleParallel(query);
            Dependency.Complete();

            UpdateECB();
            ecb = CreateEntityCommandBufferParallel();

            Entities.ForEach((int entityInQueryIndex, Entity entity, ref PathComponent pathComponent, in EvaluationResult evaluationResult, in EvaluationInputData evaluationInputData, in CurrentTileComponent currentTileComponent) =>
            {
                pathComponent.pathIndex++;
                if (pathComponent.behvaiour == evaluationResult.aiBehvaiour
                    && pathComponent.CurrentPathPosition.ToTileData().IsInRangeOfOne(currentTileComponent.CurrentTile)
                    && pathComponent.CurrentPathPosition.ToTileData().isWalkableBurstSafe(evaluationInputData.walkabilityData, evaluationInputData.currentTile.position))
                {
                }
                else
                {
                    var findPathComponent = BehaviourDatabase.GetBehaviour(evaluationResult.aiBehvaiour).GetFindPathComponent(evaluationInputData);
                    findPathComponent.behvaiour = evaluationResult.aiBehvaiour;
                    ecb.AddComponent(entityInQueryIndex, entity, new FindPathTag());
                    pathComponent = findPathComponent;
                }
            }).WithoutBurst().ScheduleParallel();

            UpdateECB();

            Entities.ForEach((int entityInQueryIndex, Entity entity, in EvaluationResult evaluationResult) =>
            {
                NewDebugMessage("\n " + entity.GetName() + " chooses " + evaluationResult.aiBehvaiour);
            }).WithoutBurst().Run();

            WriteDebug();
        }
    }
}

public partial struct GetEvaluationDataJob : IJobEntity
{
    [ReadOnly] public BufferLookup<AbilityElement> abilityBuffer;
    [ReadOnly] public ComponentLookup<CurrentTileComponent> currentTileFromEntity;
    [ReadOnly] public ComponentLookup<RangedWeaponComponent> rangedWeaponFromEntity;
    [ReadOnly] public ComponentLookup<SimpleObjectNameComponent> idComponentFromEntity;
    [ReadOnly] public ComponentLookup<PolearmTag> polearmTagFromEntity;
    [ReadOnly] public ComponentLookup<MoraleComponent> moraleComponentFromEntity;
    [ReadOnly] public Unity.Mathematics.Random random;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(
        Entity entity,
        [EntityIndexInQuery] int entityInQueryIndex,
        in SquadMemberComponent squadMemberComponent,
        in EquipmentComponent equipmentComponent,
        in AIComponent aIComponent,
        in CreatureComponent creatureComponent,
        in InventoryComponent inventoryComponent,
        in WalkabilityDataComponent walkabilityDataComponent,
        in CurrentTileComponent currentTileComponent,
        in DynamicBuffer<InventoryBufferElement> inventoryBufferElements
        )
    {
        var evaluationData = new EvaluationInputData();
        var idcomponents = idComponentFromEntity;
        var rangeWeapons = rangedWeaponFromEntity;

        evaluationData.self = entity;
        evaluationData.isPlayerSquadmate = squadMemberComponent.squadIndex == SquadsRegister.PlayerSquadIndex;
        evaluationData.homeTile = aIComponent.homePoint.ToTileData();
        evaluationData.target = aIComponent.target != null ? aIComponent.target : Entity.Null;
        evaluationData.ItemInMainHand = equipmentComponent.itemInMainHand;
        evaluationData.currentTile = currentTileComponent.currentTileId.ToTileData();
        evaluationData.targetTile = aIComponent.target != Entity.Null ? currentTileFromEntity[aIComponent.target].currentTileId.ToTileData() : TileData.Null;
        evaluationData.squadLeaderTile = currentTileFromEntity[Registers.SquadsRegister.GetSquadLeader(squadMemberComponent.squadIndex)].currentTileId.ToTileData();
        evaluationData.random = new Unity.Mathematics.Random(random.NextUInt());
        evaluationData.movementCost = creatureComponent.baseMovementCost;
        evaluationData.morale = entity.HasComponent<MoraleComponent>() ? moraleComponentFromEntity[entity].currentMorale : 100;
        evaluationData.walkabilityData = walkabilityDataComponent;
        evaluationData.hasPolearm = evaluationData.ItemInMainHand != Entity.Null ? polearmTagFromEntity.HasComponent(evaluationData.ItemInMainHand) : false;
        evaluationData.hasRangedWeapon = evaluationData.ItemInMainHand != Entity.Null ? rangedWeaponFromEntity.HasComponent(evaluationData.ItemInMainHand) : false;
        evaluationData.rangedWeaponLoaded = evaluationData.hasRangedWeapon && rangedWeaponFromEntity[evaluationData.ItemInMainHand].Ready;
        evaluationData.orderComponent = entity.HasComponent<OrderComponent>() ? entity.GetComponentData<OrderComponent>() : OrderComponent.Null;

        if (evaluationData.hasRangedWeapon)
        {
            foreach (var item in inventoryBufferElements)
            {
                if (idComponentFromEntity[item.entity].simpleObjectName == rangedWeaponFromEntity[evaluationData.ItemInMainHand].Ammo)
                {
                    evaluationData.hasAmmo = true;
                    break;
                }
            }
        }

        if (evaluationData.ItemInMainHand != Entity.Null)
        {
            foreach (var item in inventoryBufferElements)
            {
                if (idComponentFromEntity[item.entity].simpleObjectName == idComponentFromEntity[evaluationData.ItemInMainHand].simpleObjectName)
                {
                    evaluationData.canThrow = true;
                    break;
                }
            }
        }

        evaluationData.hasClearTraectory = ((evaluationData.hasRangedWeapon && evaluationData.rangedWeaponLoaded && evaluationData.hasAmmo) || evaluationData.canThrow)
            && evaluationData.target != Entity.Null
            ? evaluationData.currentTile.ClearTraectory(evaluationData.targetTile)
            : false;

        ecb.AddComponent(entityInQueryIndex, entity, evaluationData);
        ecb.RemoveComponent<EvaluateBehaviourTag>(entityInQueryIndex, entity);
    }
}