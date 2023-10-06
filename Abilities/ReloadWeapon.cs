using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Abilities
{
    [Serializable]
    public class ReloadWeapon : Ability
    {
        public override AbilityName AbilityName => AbilityName.ReloadWeapon;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var equipment = self.GetComponentData<EquipmentComponent>();

            var rangedWeaponComponent = equipment.itemInMainHand.GetComponentData<RangedWeaponComponent>();
            if (!rangedWeaponComponent.Ready)
            {
                rangedWeaponComponent.CurrentReloadPhase = math.min(rangedWeaponComponent.CurrentReloadPhase + 1, rangedWeaponComponent.MaxReloadingPhases);
                ecb.SetComponent(equipment.itemInMainHand, rangedWeaponComponent);

                if (self.IsPlayer())
                {
                    if (rangedWeaponComponent.Ready) PopUpCreator.CreatePopUp(self.CurrentTile().position, "Loaded!", Color.yellow);
                    else
                    {
                        PopUpCreator.CreatePopUp(self.CurrentTile().position, PopupType.Interaction);
                    }
                }
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