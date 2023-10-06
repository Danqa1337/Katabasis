using System;
using System.Linq;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class Kick : Ability
    {
        public override AbilityName AbilityName => AbilityName.Kick;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            ecb.AddBufferElement(self, new EffectElement(EffectName.EngagedInBattle, 25, self));
            var selfCurrentTile = self.CurrentTile();

            var target = abilityComponent.targetTile.SolidLayer;
            if (target == Entity.Null && abilityComponent.targetTile.DropLayer.Length > 0) target = abilityComponent.targetTile.DropLayer.First();

            if (target != Entity.Null)
            {
                var V = StatsCalculator.CalculateBonusDamage(self, self.GetComponentData<AnatomyComponent>().GetUnarmedAttackBodyPart());

                var H = 1;
                var axelerationVector = target.CurrentTile() - self.CurrentTile();

                ecb.AddComponent(target, new ImpulseComponent(axelerationVector, V, H, self));
                ecb.AddBufferElement(target, new EffectElement(EffectName.Stun, 30, self));
            }
            ecb.AddBufferElement(self, new MajorAnimationElement(selfCurrentTile, abilityComponent.targetTile, AnimationType.Butt));
            SoundSystem.ScheduleSound(SoundName.Woosh, selfCurrentTile);
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetTile.IsInRangeOfOne(self.CurrentTile());
        }
    }
}