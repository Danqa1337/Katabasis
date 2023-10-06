using System;
using System.Linq;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class ToggleMechansim : Ability
    {
        public override AbilityName AbilityName => AbilityName.ToggleMechanism;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            if (abilityComponent.targetTile.SolidLayer.HasComponent<MechanismTag>())
            {
                ecb.AddComponent(abilityComponent.targetTile.SolidLayer, new ToggleMechanismTag());
            }
            else if (abilityComponent.targetTile.GroundCoverLayer.Any(g => g.HasComponent<MechanismTag>()))
            {
                ecb.AddComponent(abilityComponent.targetTile.GroundCoverLayer.First(g => g.HasComponent<MechanismTag>()), new ToggleMechanismTag());
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            if (abilityComponent.targetTile.GetNeibors(true).Contains(self.CurrentTile()))
            {
                var objects = abilityComponent.targetTile.GroundCoverLayer.ToList();
                objects.Add(abilityComponent.targetTile.SolidLayer);
                return objects.Any(e => e.HasComponent<MechanismTag>());
            }
            return false;
        }
    }
}