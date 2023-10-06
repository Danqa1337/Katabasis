using System;
using System.Linq;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class Attack : Ability
    {
        public override AbilityName AbilityName => AbilityName.Attack;

        public Entity GetWeapon(Entity self, AbilityComponent abilityComponent)
        {
            var equip = self.GetComponentData<EquipmentComponent>();
            var creatureComponent = self.GetComponentData<CreatureComponent>();
            if (equip.itemInMainHand != Entity.Null)
            {
                return equip.itemInMainHand;
            }
            else if (equip.itemInOffHand != Entity.Null)
            {
                return equip.itemInOffHand;
            }
            else
            {
                return self.GetComponentData<AnatomyComponent>().GetUnarmedAttackBodyPart();
            }
        }

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var selfCurrentTile = self.CurrentTile();
            var weapon = GetWeapon(self, abilityComponent);

            var weaponName = weapon.GetName();

            ecb.AddComponent(weapon, self.GetComponentData<CurrentTileComponent>());

            if (debug) AddToLastDebugMessage(" with " + weaponName);

            ecb.AddBufferElement(self, new MajorAnimationElement(self.CurrentTile(), abilityComponent.targetTile, AnimationType.Butt));
            SoundSystem.ScheduleSound(SoundName.Woosh, selfCurrentTile);
            ecb.AddBufferElement(self, new EffectElement(EffectName.EngagedInBattle, 25, self));

            var targetEntity = abilityComponent.targetEntity;
            if (targetEntity == Entity.Null)
            {
                if (abilityComponent.targetTile.SolidLayer != Entity.Null)
                {
                    targetEntity = abilityComponent.targetTile.SolidLayer;
                }
                else
                {
                    targetEntity = abilityComponent.targetTile.DropLayer.FirstOrDefault();
                }
            }

            if (targetEntity != Entity.Null)
            {
                if (debug) AddToLastDebugMessage(" on " + targetEntity.GetName());

                if (targetEntity.HasComponent<MechanismTag>())
                {
                    new ToggleMechansim().DoAction(self, abilityComponent, ecb);
                }
                else
                {
                    if (targetEntity.GetComponentData<ObjectTypeComponent>().objectType == ObjectType.Drop || BaseMethodsClass.Chance(StatsCalculator.CalculateTOH(self, weapon)))
                    {
                        var bonusDamage = (int)StatsCalculator.CalculateBonusDamage(self, weapon);

                        var collisionElement = new CollisionElement(weapon, self, bonusDamage);
                        ecb.AddBufferElement(targetEntity, collisionElement);
                        foreach (var perkElement in self.GetBuffer<PerkElement>())
                        {
                            PerksDatabase.GetPerk(perkElement.PerkName).OnAttack(self, targetEntity, ecb);
                        }
                    }
                    else
                    {
                        if (debug) AddToLastDebugMessage(" ...but misses it");
                    }
                }
            }
            else
            {
                if (debug) AddToLastDebugMessage(" but hits nothing");
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            var weapon = GetWeapon(self, abilityComponent);
            return StatsCalculator.CalculateAttackCost(self, weapon);
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            var polearm = GetWeapon(self, abilityComponent).HasComponent<PolearmTag>();
            return polearm ? abilityComponent.targetTile.IsInRangeOfTwo(self.CurrentTile()) : abilityComponent.targetTile.IsInRangeOfOne(self.CurrentTile());
        }
    }
}