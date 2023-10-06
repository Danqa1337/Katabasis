using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class OderGoTo : Ability
    {
        public override AbilityName AbilityName => AbilityName.OrderGoTo;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            foreach (var squadMate in Registers.SquadsRegister.GetSquadmates(self.GetComponentData<SquadMemberComponent>().squadIndex))
            {
                if (squadMate != self)
                {
                    ecb.AddComponent(squadMate, new OrderComponent(OrderType.GoTo, abilityComponent.targetTile));
                }
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            if (Registers.SquadsRegister.GetSquadmates(self.GetComponentData<SquadMemberComponent>().squadIndex).Count > 1)
            {
                if(abilityComponent.targetTile.visible && abilityComponent.targetTile.SolidLayer == Entity.Null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}