using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class AttackAimed : Ability
    {
        public override AbilityName AbilityName => AbilityName.AttackAimed;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            new Attack().DoAction(self, abilityComponent, ecb);
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return new Attack().AdditionalCooldown(self, abilityComponent);
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return new Attack().TileFunc(self, abilityComponent) && abilityComponent.targetTile.SolidLayer != Entity.Null;
        }
    }
}