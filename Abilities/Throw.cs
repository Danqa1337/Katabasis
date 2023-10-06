using System;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Abilities
{
    [Serializable]
    public class Throw : Ability
    {
        public override AbilityName AbilityName => AbilityName.Throw;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var selfCurrentTile = self.CurrentTile();
            var inventory = self.GetComponentData<InventoryComponent>();
            ecb.AddBufferElement(self, new EffectElement(EffectName.EngagedInBattle, 25, self));

            var bonusDamage = (int)((StatsCalculator.CalculateBonusDamage(self, abilityComponent.targetEntity) * abilityComponent.targetEntity.GetComponentData<PhysicsComponent>().aerodynamicsDamageMultiplier));
            var H = Mathf.RoundToInt(selfCurrentTile.GetDistance(abilityComponent.targetTile) - 0.4f);
            var axelerationVector = math.normalize(abilityComponent.targetTile - self.CurrentTile());
            axelerationVector += UnityEngine.Random.insideUnitCircle.ToFloat2() * LowLevelSettings.instance.baseRangedInaccuracity;

            ecb.AddComponent(abilityComponent.targetEntity, new ImpulseComponent(axelerationVector, bonusDamage, H, self));
            ecb.AddBufferElement(self, new ChangeEquipmentElement(Entity.Null, EquipTag.Weapon));
            ecb.AddBufferElement(self, new MajorAnimationElement(selfCurrentTile, abilityComponent.targetTile, AnimationType.Butt));

            if (inventory.items.Count > 0)
            {
                var nextItem = inventory.items.First();
                var itemWithSameId = inventory.FindItem(abilityComponent.targetEntity.GetItemName());
                var equipTag = EquipTag.Weapon;
                if (itemWithSameId != Entity.Null)
                {
                    nextItem = itemWithSameId;
                }
                if (self.GetComponentData<AnatomyComponent>().CanHold(EquipTag.Weapon))
                {
                    equipTag = EquipTag.Weapon;
                }
                else if (self.GetComponentData<AnatomyComponent>().CanHold(EquipTag.Shield))
                {
                    equipTag = EquipTag.Shield;
                }
                else
                {
                    throw new Exception("thrown an item but can not hold anything");
                }

                ecb.AppendToBuffer(self, new ChangeEquipmentElement(nextItem, equipTag));
                self.GetBuffer<InventoryBufferElement>().Remove(new InventoryBufferElement(nextItem));
            }
            SoundSystem.ScheduleSound(SoundName.Woosh, selfCurrentTile);
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            var cost = StatsCalculator.CalculateAttackCost(self, abilityComponent.targetEntity) * 1.5f;

            return (int)cost;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetTile != self.CurrentTile();
        }
    }
}