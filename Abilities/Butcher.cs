using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class Butcher : Ability
    {
        public override AbilityName AbilityName => AbilityName.Butcher;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var selfCurrentTile = self.CurrentTile();

            foreach (var item in selfCurrentTile.DropLayer)
            {
                if (item.HasTag(Tag.Butcherable))
                {
                    ecb.AddComponent(item, new BreakObjectComponent(self, DurabilityChangeReason.Smashed));
                    //PopUpCreator.CreatePopUp(self.GetComponentObject<EntityAuthoring>().transform, "butcher " + item.GetName());
                    return;
                }
            }

            PopUpCreator.CreatePopUp(self.CurrentTile().position, LocalizationManager.GetString("There is nothing to eat"));
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 20;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return true;
        }
    }
}