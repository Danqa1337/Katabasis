using Assets.Scripts;
using System;
using System.Linq;
using Unity.Entities;

namespace Abilities
{
    public enum AbilityTargeting
    {
        Instant,
        OnTile,
        OnEntity,
        OnSelf,
        OrderOnTile,
    }

    [Serializable]
    public class Amputate : Ability
    {
        public override AbilityName AbilityName => AbilityName.Amputate;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            Surgeon.Amputate(abilityComponent.targetTile.SolidLayer, abilityComponent.targetEntity, self);
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 10;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetTile.SolidLayer != Entity.Null && (abilityComponent.targetTile.GetNeibors(true).Contains(self.CurrentTile()) || abilityComponent.targetTile == self.CurrentTile());
        }
    }

    [Serializable]
    public class Blink : Ability
    {
        public override AbilityName AbilityName => AbilityName.Blink;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var tile = self.CurrentTile().GetTilesInRadius(4).FirstOrDefault(t => t.isWalkable(self));
            if (tile != TileData.Null)
            {
                ecb.AddComponent(self, new MoveComponent(self.CurrentTile(), tile, MovemetType.PositionChange));
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 10;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return true;
        }
    }

    [Serializable]
    public class SelfHarm : Ability
    {
        public override AbilityName AbilityName => AbilityName.SelfHarm;

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var duarbility = self.GetComponentData<DurabilityComponent>().currentDurability;
            ecb.AddBufferElement(self, new DurabilityChangeElement(-(int)MathF.Min(10, duarbility - 1), self, DurabilityChangeReason.InstantDamage));
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return true;
        }
    }

    [Serializable]
    public class HeadButt : Ability
    {
        public override AbilityName AbilityName => AbilityName.HeadButt;

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var selfCurrentTile = self.CurrentTile();
            var head = self.GetComponentData<AnatomyComponent>().Head;
            var helmet = self.GetComponentData<EquipmentComponent>().itemOnHead;

            ecb.AddComponent(head, self.GetComponentData<CurrentTileComponent>());

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
                    if (targetEntity.GetComponentData<ObjectTypeComponent>().objectType == ObjectType.Drop || BaseMethodsClass.Chance(StatsCalculator.CalculateTOH(self, head)))
                    {
                        var bonusDamage = (int)StatsCalculator.CalculateBonusDamage(self, head) + 25;
                        if (helmet != Entity.Null) bonusDamage += StatsCalculator.CalculateBonusDamage(self, helmet) + helmet.GetComponentData<PhysicsComponent>().damage;
                        var collisionElement = new CollisionElement(head, self, bonusDamage);
                        ecb.AddBufferElement(targetEntity, collisionElement);
                        foreach (var perkElement in self.GetBuffer<PerkElement>())
                        {
                            PerksDatabase.GetPerk(perkElement.PerkName).OnAttack(self, targetEntity, ecb);
                        }
                        if (KatabasisUtillsClass.Chance(50))
                        {
                            ecb.AddBufferElement(targetEntity, new EffectElement(EffectName.Stun, 20, self));
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

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetTile.SolidLayer != Entity.Null && (abilityComponent.targetTile.GetNeibors(true).Contains(self.CurrentTile()));
        }
    }
}