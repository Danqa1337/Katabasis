using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public partial class AiCooldownSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();

        Entities.WithAll<AIComponent>().WithNone<ImpulseComponent, AiReadyTag>().ForEach((int entityInQueryIndex, Entity entity, ref AIComponent aIComponent) =>
        {

            aIComponent.abilityCooldown = Mathf.Max(aIComponent.abilityCooldown - 1, 0);
            aIComponent.targetSearchCooldown = Mathf.Max(aIComponent.targetSearchCooldown - 1, 0);
            aIComponent.agressionCooldown = Mathf.Max(aIComponent.agressionCooldown - 1, 0);

            if (aIComponent.AbilityReady)
            {
                ecb.AddComponent(entityInQueryIndex, entity, new AiReadyTag());
            }
            if (aIComponent.targetSearchCooldown == 0)
            {
                ecb.AddComponent(entityInQueryIndex, entity, new FindNewTargetTag());
                aIComponent.targetSearchCooldown = 30;
            }
            if (aIComponent.agressionCooldown == 0)
            {

                if (aIComponent.target == Entity.Null)
                {
                    ecb.AddComponent(entityInQueryIndex, entity, new FindAgressionTargetTag());
                }

                aIComponent.agressionCooldown = 50;
            }

        }).WithBurst().ScheduleParallel();

        UpdateECB();
    }
}
