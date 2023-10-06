using System.Collections;
using System.Drawing.Text;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using static UnityEditor.Progress;

[DisableAutoCreation]
public partial class FireSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var random = GetRandom();
        var ecb = CreateEntityCommandBufferParallel();
        Entities.WithAll<FireComponent>().ForEach((int entityInQueryIndex, Entity entity, in CurrentTileComponent currentTileComponent) =>
        {
            var affectedItems = currentTileComponent.CurrentTile.DropLayer.ToList();
            if (currentTileComponent.CurrentTile.SolidLayer != Entity.Null)
            {
                affectedItems.Add(currentTileComponent.CurrentTile.SolidLayer);
            }

            foreach (var item in affectedItems)
            {
                ecb.AddComponent(entityInQueryIndex, item, new OnFireTag());
            }
        }).WithoutBurst().ScheduleParallel();

        UpdateECB();
        ecb = CreateEntityCommandBufferParallel();

        Entities.WithAll<OnFireTag>().ForEach((int entityInQueryIndex, Entity entity, in DurabilityComponent durabilityComponent, in CurrentTileComponent currentTileComponent) =>
        {
            ecb.RemoveComponent<OnFireTag>(entityInQueryIndex, entity);
            var damage = 10 + (int)KatabasisUtillsClass.Percent(durabilityComponent.MaxDurability, 1);
            ecb.AddBufferElement(entityInQueryIndex, entity, new DurabilityChangeElement(-damage, entity, DurabilityChangeReason.Burned));

            if (KatabasisUtillsClass.ChanceThreadSafe(random, 3) || damage >= durabilityComponent.currentDurability)
            {
                SpawnSystem.ScheduleSpawn(SimpleObjectName.Smoke, currentTileComponent.CurrentTile.GetNeibors(true).RandomItemThreadSafe(random));
            }
        }).WithoutBurst().ScheduleParallel();

        UpdateECB();
    }
}

public struct FireComponent : IComponentData
{
}

public struct OnFireTag : IComponentData
{
}