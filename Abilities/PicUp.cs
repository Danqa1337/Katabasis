using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class PicUp : Ability
    {
        public override AbilityName AbilityName => AbilityName.PicUp;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            abilityComponent.targetEntity.CurrentTile().Remove(abilityComponent.targetEntity);

            if (!abilityComponent.targetEntity.HasComponent<ImpulseComponent>())
            {
                ecb.AddBufferElement(self, new ChangeInventoryElement(abilityComponent.targetEntity, true));
            }
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