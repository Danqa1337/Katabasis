using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class SpendTime1 : Ability
    {
        public override AbilityName AbilityName => AbilityName.SpendTime1;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            throw new System.NotImplementedException();
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