using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public struct HasReadyEffectTag : IComponentData
{
}

public struct HasEndedEffectTag : IComponentData
{
}

public struct HasNewEffectTag : IComponentData
{
}

[DisableAutoCreation]
public partial class EffectCoooldownSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();

        Entities.ForEach((int entityInQueryIndex, Entity entity, ref DynamicBuffer<EffectElement> buffer) =>
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                var effect = buffer[i];

                if (effect.duration != 0)
                {
                    if (effect.duration > 0) effect.duration--;

                    ecb.AddComponent(entityInQueryIndex, entity, new HasReadyEffectTag());

                    buffer[i] = effect;
                }
                else // end effect
                {
                    ecb.AddComponent(entityInQueryIndex, entity, new HasEndedEffectTag());
                }

                if (effect.activated)
                {
                    ecb.AddComponent(entityInQueryIndex, entity, new HasNewEffectTag());
                }
            }
        }).WithBurst().ScheduleParallel();

        UpdateECB();
    }
}

[DisableAutoCreation]
public partial class EffectActivationSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        Entities.WithAll<HasNewEffectTag>().ForEach((Entity entity, ref DynamicBuffer<EffectElement> effectElements) =>
        {
            for (int i = 0; i < effectElements.Length; i++)
            {
                var effectElement = effectElements[i];
                if (!effectElement.activated)
                {
                    effectElement.activated = true;
                    EffectsDatabase.GetEffect(effectElement.EffectName).OnApply(entity, effectElement, ecb);
                }
                effectElements[i] = effectElement;
            }
        }).WithoutBurst().Run();
        UpdateECB();
    }
}

[DisableAutoCreation]
public partial class EffectRealisationSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        Entities.ForEach((Entity entity, in DynamicBuffer<EffectElement> buffer) =>
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                var effectElement = buffer[i];
                var effect = EffectsDatabase.GetEffect(effectElement.EffectName);
                effect.OnTick(entity, effectElement, ecb);
            }
        }).WithoutBurst().Run();

        UpdateECB();
    }
}

[DisableAutoCreation]
public partial class EffectRemovingSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var removeQuery = GetEntityQuery(ComponentType.ReadOnly<HasEndedEffectTag>());
        var ecb = CreateEntityCommandBuffer();
        if (!removeQuery.IsEmpty)
        {
            var array = removeQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in array)
            {
                entity.RemoveComponent<HasEndedEffectTag>();

                var effectBuffer = GetBufferLookup<EffectElement>()[entity];
                var effectsToRemove = new List<EffectName>();
                for (int i = 0; i < effectBuffer.Length; i++)
                {
                    var effectElement = effectBuffer[i];

                    if (effectBuffer[i].duration == 0)
                    {
                        EffectsDatabase.GetEffect(effectElement.EffectName).OnEnd(entity, effectElement, ecb);
                        effectsToRemove.Add(effectElement.EffectName);
                    }
                }
                if (effectBuffer.Length == effectsToRemove.Count)
                {
                    entity.RemoveBuffer<EffectElement>();
                }
                else
                {
                    foreach (var effectType in effectsToRemove)
                    {
                        effectBuffer = GetBufferLookup<EffectElement>()[entity];
                        foreach (var item in effectBuffer)
                        {
                            if (item.EffectName == effectType)
                            {
                                effectBuffer.Remove(item);
                                break;
                            }
                        }
                    }
                }
            }

            array.Dispose();
        }
        UpdateECB();
    }
}

[System.Serializable]
public struct EffectElement : IBufferElementData
{
    public readonly EffectName EffectName;
    public readonly int Level;
    [NonSerialized] public readonly Entity ResponsibleEntity;
    public bool activated;
    public int duration;

    public EffectElement(EffectName effectType, int duration, Entity responsibleEntity, int level = 0)
    {
        EffectName = effectType;
        Level = level;
        ResponsibleEntity = responsibleEntity;
        activated = false;
        this.duration = duration;
    }
}

[System.Serializable]
public struct EffectOnHitElement : IBufferElementData
{
    public readonly EffectName EffectName;

    public EffectOnHitElement(EffectName effect)
    {
        this.EffectName = effect;
    }
}