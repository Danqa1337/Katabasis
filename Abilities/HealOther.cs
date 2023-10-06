using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class HealOther : Ability
    {
        public override AbilityName AbilityName => AbilityName.HealOther;

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
            var selfCurrentTile = self.CurrentTile();
            if (abilityComponent.targetTile != selfCurrentTile && abilityComponent.targetTile.IsInRangeOfOne(selfCurrentTile))
            {
                var targetEntity = abilityComponent.targetTile.SolidLayer;
                if (targetEntity != Entity.Null && Registers.SquadsRegister.GetSquadmates(self.GetComponentData<SquadMemberComponent>().squadIndex).Contains(targetEntity))
                {
                    return true;
                }
            }
            return false;
        }
    }
}