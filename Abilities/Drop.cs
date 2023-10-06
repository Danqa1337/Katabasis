using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class Drop : Ability
    {
        public override AbilityName AbilityName => AbilityName.Drop;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var item = abilityComponent.targetEntity;
            var inventory = self.GetComponentData<InventoryComponent>();
            var equipment = self.GetComponentData<EquipmentComponent>();

            if (abilityComponent.targetEntity == Entity.Null) throw new Exception(self.GetName() + " is trying to drop null");
            if (!abilityComponent.targetEntity.Exists()) throw new Exception(self.GetName() + "is  trying to drop entity that not exist");

            if (equipment.GetEquipmentNotNull().Contains(item))
            {
                ecb.AddBufferElement(self, new ChangeEquipmentElement(Entity.Null, equipment.GetEquipTag(item)));
            }
            else
            {
                if (inventory.items.Contains(item))
                {
                    ecb.AddBufferElement(self, new ChangeInventoryElement(item, false));
                }
                else
                {
                    throw new Exception(item.GetName() + " is not present in equip nor inventory of " + self.GetName());
                }
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetEntity != Entity.Null;
        }
    }
}