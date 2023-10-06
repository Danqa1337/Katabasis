using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisableAutoCreation]
public partial class MoraleSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();
        var random = GetRandom();
        Entities.ForEach((int entityInQueryIndex, Entity entity, ref MoraleComponent moraleComponent, in DynamicBuffer<MoraleChangeElement> moraleChangeBuffer) =>
        {
            var moraleChange = 0f;
            foreach (var item in moraleChangeBuffer)
            {
                moraleChange += item.value;
            }
            moraleComponent.currentMorale = Mathf.Clamp(moraleComponent.currentMorale + moraleChange, 0, 100);
        }).WithoutBurst().ScheduleParallel();

        Entities.ForEach((Entity entity, ref MoraleComponent moraleComponent, in DynamicBuffer<HealthChangedElement> healthChangeBuffer, in DurabilityComponent durabilityComponent, in AIComponent aIComponent) =>
        {
            float healthChangeSum = 0f;

            foreach (var item in healthChangeBuffer)
            {
                healthChangeSum += item.value;
            }
            var moraleChange = 100f * (healthChangeSum / (float)durabilityComponent.MaxDurability);

            moraleComponent.currentMorale = Mathf.Clamp(moraleComponent.currentMorale + moraleChange, 0, 100);
        }).WithoutBurst().ScheduleParallel();

        Entities.WithAll<FleingTag>().ForEach((int entityInQueryIndex, Entity entity, in MoraleComponent moraleComponent) =>
        {
            if (moraleComponent.currentMorale > AiSystem.moraleFleeTreshold)
            {
                ecb.AddBufferElement(entityInQueryIndex, entity, new ChangeOverHeadAnimationElement(OverHeadAnimationType.Fear, false));
                ecb.RemoveComponent<FleingTag>(entityInQueryIndex, entity);
            }
        }).WithoutBurst().ScheduleParallel();

        Entities.WithNone<FleingTag>().ForEach((int entityInQueryIndex, Entity entity, in MoraleComponent moraleComponent) =>
        {
            if (moraleComponent.currentMorale < AiSystem.moraleFleeTreshold)
            {
                ecb.AddBufferElement(entityInQueryIndex, entity, new ChangeOverHeadAnimationElement(OverHeadAnimationType.Fear, true));
                ecb.AddComponent(entityInQueryIndex, entity, new FleingTag());
            }
        }).WithoutBurst().ScheduleParallel();

        Entities.ForEach((int entityInQueryIndex, Entity entity, in DynamicBuffer<MoraleChangeElement> moraleChangeBuffer) =>
        {
            ecb.RemoveComponent<MoraleChangeElement>(entityInQueryIndex, entity);
        }).WithoutBurst().ScheduleParallel();
        UpdateECB();

        var ecbSingle = CreateEntityCommandBuffer();

        Entities.WithAll<FleingTag>().ForEach((Entity entity, in AnatomyComponent anatomyComponent) =>
        {
            foreach (var item in anatomyComponent.GetBodyPartsNotNull())
            {
                var durabilityComponent = item.GetComponentData<DurabilityComponent>();
                if (KatabasisUtillsClass.Chance(8) && (float)durabilityComponent.currentDurability / (float)durabilityComponent.MaxDurability <= 0.5f)
                {
                    ecbSingle.AddBufferElement(item, new DurabilityChangeElement(5, entity, DurabilityChangeReason.Healed));
                }
            }
        }).WithoutBurst().Run();
        UpdateECB();
    }
}

[System.Serializable]
public struct MoraleComponent : IComponentData
{
    public float currentMorale;

    public MoraleComponent(float currentMorale)
    {
        this.currentMorale = currentMorale;
    }

    public bool isFleeing => currentMorale < AiSystem.moraleFleeTreshold;
}

public struct MoraleChangeElement : IBufferElementData
{
    public float value;

    public MoraleChangeElement(float value)
    {
        this.value = value;
    }
}

public struct FleingTag : IComponentData
{
}