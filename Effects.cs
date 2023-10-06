using System.Collections;
using Unity.Entities;
using UnityEngine;
using System;

namespace Effects
{
    [Serializable]
    public abstract class Effect
    {
        public abstract EffectName EffectName { get; }

        public abstract void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb);

        public abstract void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb);

        public abstract void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb);
    }

    [Serializable]
    public class Regeneration : Effect
    {
        public override EffectName EffectName => EffectName.Regeneration;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
            ecb.AddBufferElement(entity, new DurabilityChangeElement(1, effectElement.ResponsibleEntity, DurabilityChangeReason.Healed));
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class Bleeding : Effect
    {
        public override EffectName EffectName => EffectName.Bleeding;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
            if (entity.HasComponent<InternalLiquidComponent>() && KatabasisUtillsClass.Chance(8))
            {
                ecb.AddComponent(entity, new SpillLiquidComponent(0, 1));
            }
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class Haste : Effect
    {
        public override EffectName EffectName => EffectName.Haste;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class Slowness : Effect
    {
        public override EffectName EffectName => EffectName.Slowness;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class ArmourIgnoring : Effect
    {
        public override EffectName EffectName => EffectName.ArmourIgnoring;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class Stun : Effect
    {
        public override EffectName EffectName => EffectName.Stun;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
            if (entity.HasComponent<AIComponent>())
            {
                var ai = entity.GetComponentData<AIComponent>();
                ai.abilityCooldown += effectElement.duration;
                entity.SetComponentData(ai);
                entity.AddBufferElement(new ChangeOverHeadAnimationElement(OverHeadAnimationType.Stun, true));
            }
            else if (entity.IsPlayer())
            {
                TimeController.SpendTime(effectElement.duration);
                entity.AddBufferElement(new ChangeOverHeadAnimationElement(OverHeadAnimationType.Stun, true));
            }
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
            ecb.AddBufferElement(entity, new ChangeOverHeadAnimationElement(OverHeadAnimationType.Stun, false));
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class Invisibility : Effect
    {
        public override EffectName EffectName => EffectName.Invisibility;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class Petrification : Effect
    {
        public override EffectName EffectName => EffectName.Petrification;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class EngagedInBattle : Effect
    {
        public override EffectName EffectName => EffectName.EngagedInBattle;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class MoraleRegen : Effect
    {
        public override EffectName EffectName => EffectName.MoraleRegen;

        public override void OnApply(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnEnd(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
        }

        public override void OnTick(Entity entity, EffectElement effectElement, EntityCommandBuffer ecb)
        {
            ecb.AddBufferElement(entity, new MoraleChangeElement(1));
        }
    }
}