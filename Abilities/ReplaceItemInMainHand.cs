using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class ReplaceItemInMainHand : Ability
    {
        public override AbilityName AbilityName => AbilityName.ReplaceItemInMainHand;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            new PicUp().DoAction(self, abilityComponent, ecb);
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