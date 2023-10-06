using Unity.Entities;

[DisableAutoCreation]
public partial class ExecuteBehaviourSystem : SystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnUpdate()
    {
        var ecb = _manualCommanBufferSytem.CreateCommandBuffer().AsParallelWriter();
        Entities.ForEach((int entityInQueryIndex, Entity entity, in EvaluationInputData inputData, in EvaluationResult evaluationResult, in PathComponent findPathComponent) =>
        {
            var ability = BehaviourDatabase.GetBehaviour(evaluationResult.aiBehvaiour).GetAbility(inputData, findPathComponent);

            ecb.AddComponent(entityInQueryIndex, entity, ability);
            ecb.RemoveComponent<EvaluationInputData>(entityInQueryIndex, entity);
            ecb.RemoveComponent<EvaluationResult>(entityInQueryIndex, entity);
        }).WithoutBurst().ScheduleParallel();

        Dependency.Complete();
        _manualCommanBufferSytem.Update();
    }
}