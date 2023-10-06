using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class HealSelf : Ability
    {
        public override AbilityName AbilityName => AbilityName.HealSelf;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var power = AbilitiesDatabase.GetAbilityData(AbilityName).Power;
            ecb.AddBufferElement(abilityComponent.targetEntity, new DurabilityChangeElement((int)power, self, DurabilityChangeReason.Healed));
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return true;
        }
    }
}