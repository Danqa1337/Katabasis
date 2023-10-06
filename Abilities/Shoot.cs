using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Abilities
{
    [Serializable]
    public class Shoot : Ability
    {
        public override AbilityName AbilityName => AbilityName.Shoot;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var selfCurrentTile = self.CurrentTile();
            var inventory = self.GetComponentData<InventoryComponent>();
            var equipment = self.GetComponentData<EquipmentComponent>();

            if (equipment.itemInMainHand.HasComponent<RangedWeaponComponent>())
            {
                var rangedWeaponComponent = equipment.itemInMainHand.GetComponentData<RangedWeaponComponent>();
                var projectile = inventory.FindItem(SimpleObjectName.Arrow);
                if (projectile != Entity.Null)
                {
                    if (rangedWeaponComponent.Ready)
                    {
                        ecb.AddBufferElement(self, new EffectElement(EffectName.EngagedInBattle, 25, self));
                        rangedWeaponComponent.CurrentReloadPhase = 0;
                        ecb.SetComponent(equipment.itemInMainHand, rangedWeaponComponent);
                        ecb.AddBufferElement(self, new ChangeInventoryElement(projectile, false));

                        var projectilePhysics = projectile.GetComponentData<PhysicsComponent>();
                        var vector = math.normalize(abilityComponent.targetTile - selfCurrentTile) + UnityEngine.Random.insideUnitCircle.ToFloat2() * LowLevelSettings.instance.baseRangedInaccuracity;
                        var h = Mathf.RoundToInt(selfCurrentTile.GetDistance(abilityComponent.targetTile));
                        var impulseComponent = new ImpulseComponent(vector, (int)(StatsCalculator.CalculateBonusDamage(self, projectile) * projectilePhysics.aerodynamicsDamageMultiplier) + rangedWeaponComponent.Power, h, self);
                        var transform = projectile.GetComponentObject<EntityAuthoring>().transform;

                        transform.rotation = quaternion.LookRotationSafe(Vector3.forward, new Vector3(vector.x, vector.y, 0).normalized);

                        ecb.AddComponent(projectile, impulseComponent);
                    }
                    else
                    {
                        AddToLastDebugMessage(" ...but weapon need to be reloaded first");
                        new ReloadWeapon().DoAction(self, abilityComponent, ecb);
                    }
                }
                else
                {
                    PopUpCreator.CreatePopUp(self.CurrentTile().position, "I have no arrows");
                }
            }
            else
            {
                throw new Exception(equipment.itemInMainHand.GetName() + " is not ranged weapon");
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            var equipment = self.GetComponentData<EquipmentComponent>();
            return equipment.itemInMainHand.HasComponent<RangedWeaponComponent>();
        }
    }
}