using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class RestoreBodyPart : Ability
    {
        public override AbilityName AbilityName => AbilityName.RestoreBodyPart;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {

        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            if (Registers.SquadsRegister.GetSquadmates(self.GetComponentData<SquadMemberComponent>().squadIndex).Count > 1)
            {
                if (abilityComponent.targetTile.visible && abilityComponent.targetTile.SolidLayer == Entity.Null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}