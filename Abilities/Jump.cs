using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class Jump : Ability
    {
        public override AbilityName AbilityName => AbilityName.Jump;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            if (debug) AddToLastDebugMessage(self.GetName() + " jumps.");
            ecb.AddComponent(self, new MoveComponent(self.CurrentTile(), abilityComponent.targetTile, MovemetType.SelfPropeled));
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return (int)(StatsCalculator.CalculateMovementCost(self, self.CurrentTile()) * 1.5f);
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetTile.IsInRangeOfTwo(self.CurrentTile());
        }
    }
}