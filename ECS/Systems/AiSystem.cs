using Assets.Scripts;
using Unity.Entities;
using Unity.Jobs;

public enum AiBehaviourName
{
    Wait,
    Chase,
    Atack,
    Flee,
    Roam,
    StepBack,
    StepAside,
    Shoot,
    Reload,
    Throw,
    Picup,
    Follow,
    ReturnHome,
    GoToOrder,
    AttackOrder,
}

[DisableAutoCreation]
public partial class AiSystem : MySystemBase
{
    public const float moraleFleeTreshold = 30;

    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();

        Entities.WithAll<AiReadyTag>().WithNone<ImpulseComponent>().ForEach((int entityInQueryIndex, Entity self) =>
        {
            ecb.AddComponent(entityInQueryIndex, self, new EvaluateBehaviourTag());
        }).WithBurst().ScheduleParallel();
        UpdateECB();

        ecb = CreateEntityCommandBufferParallel();
        Entities.WithAll<AiReadyTag>().ForEach((int entityInQueryIndex, Entity self, ref AIComponent aIComponent) =>
        {
            if (!aIComponent.target.Exists() || !aIComponent.target.HasComponent<AliveTag>()) //delete dead targets
            {
                aIComponent.target = Entity.Null;

                ecb.AddComponent(entityInQueryIndex, self, new FindNewTargetTag());
            }
            ecb.RemoveComponent<AiReadyTag>(entityInQueryIndex, self);
        }).WithoutBurst().ScheduleParallel();

        Entities.ForEach((int entityInQueryIndex, Entity self, ref MoraleComponent moraleComponent, in AIComponent aIComponent) =>
        {
            if (aIComponent.target == Entity.Null)
            {
                moraleComponent.currentMorale += 0.2f;
            }
            else
            {
                moraleComponent.currentMorale += 0.1f;
            }
        }).WithBurst().ScheduleParallel();

        UpdateECB();
    }
}

public struct AiTargetComponent : IComponentData
{
    public Entity entity;
}