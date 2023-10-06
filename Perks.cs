using System.Collections;
using UnityEngine;
using System;
using Unity.Entities;
using Assets.Scripts;

namespace Perks
{
    [Serializable]
    public abstract class Perk
    {
        public abstract PerkName PerkName { get; }

        public virtual void Grant(Entity entity)
        {
            if (entity.HasBuffer<PerkElement>())
            {
                entity.AddBufferElement(new PerkElement(PerkName));
            }
            else
            {
                throw new System.Exception("There is no perk buffer on " + entity.GetName());
            }
        }

        public virtual void Revoke(Entity entity)
        {
            if (entity.HasBuffer<PerkElement>())
            {
                entity.GetBuffer<PerkElement>().Remove(new PerkElement(PerkName));
            }
            else
            {
                throw new System.Exception("There is no perk buffer on " + entity.GetName());
            }
        }

        public virtual void OnAttack(Entity self, Entity target, EntityCommandBuffer ecb)
        {
        }

        public virtual void OnEat(Entity self, Entity food, EntityCommandBuffer ecb)
        {
        }
    }

    [Serializable]
    public class ArmokBleeding : Perk
    {
        public override void OnAttack(Entity self, Entity target, EntityCommandBuffer ecb)
        {
            if (target.HasComponent<CreatureComponent>() && target.HasComponent<InternalLiquidComponent>())
            {
                ecb.AddBufferElement(target, new EffectElement(EffectName.Bleeding, 50, self));
            }
        }

        public override PerkName PerkName => PerkName.ArmokBleeding;
    }

    [Serializable]
    public class PushunRestoration : Perk
    {
        public override PerkName PerkName => PerkName.PushunRestoration;

        public override void OnEat(Entity self, Entity food, EntityCommandBuffer ecb)
        {
            base.OnEat(self, food, ecb);
            var missingParts = self.GetBuffer<MissingBodypartBufferElement>();
            if (missingParts.Length > 0)
            {
                if (BaseMethodsClass.Chance(20))
                {
                    var partName = missingParts[UnityEngine.Random.Range(0, missingParts.Length - 1)].ItemName;
                    var partTag = missingParts[UnityEngine.Random.Range(0, missingParts.Length - 1)].tag;
                    var creature = self;
                    var currentTile = self.CurrentTile();

                    DelayedActionsUpdaterOnCycle.ScheduleAction(delegate
                    {
                        var newPart = Spawner.Spawn(partName, currentTile);
                        currentTile.Remove(newPart);
                        var partDurability = newPart.GetComponentData<DurabilityComponent>();
                        partDurability.currentDurability = 1;
                        newPart.SetComponentData(partDurability);
                        Surgeon.SewPart(self, newPart);
                    });
                }
            }
        }
    }

    [Serializable]
    public class IronArm : Perk
    {
        public override PerkName PerkName => PerkName.IronArm;

        public override void Grant(Entity entity)
        {
            base.Grant(entity);
            var arm = Spawner.Spawn(SimpleObjectName.IronArm, entity.CurrentTile());
            var oldArm = entity.GetComponentData<AnatomyComponent>().RightArm;

            if (oldArm != Entity.Null)
            {
                oldArm.AddComponentData(new DestroyEntityTag());
            }
            Surgeon.SewPart(entity, arm);
            ManualSystemUpdater.Update<AnatomySystem>();
        }
    }

    [Serializable]
    public class IronHead : Perk
    {
        public override PerkName PerkName => PerkName.IronHead;

        public override void Grant(Entity entity)
        {
            base.Grant(entity);
            var oldHead = entity.GetComponentData<AnatomyComponent>().Head;
            if (oldHead != Entity.Null)
            {
                oldHead.AddComponentData(new DestroyEntityTag());
            }
            var head = Spawner.Spawn(SimpleObjectName.IronHead, entity.CurrentTile());
            Surgeon.SewPart(entity, head);
            ManualSystemUpdater.Update<AnatomySystem>();
        }
    }

    [Serializable]
    public class Acephalos : Perk
    {
        public override PerkName PerkName => PerkName.Acephalos;
    }

    [Serializable]
    public class GrowTail : Perk
    {
        public override PerkName PerkName => PerkName.GrowTail;

        public override void Grant(Entity entity)
        {
            base.Grant(entity);
            var tail = Spawner.Spawn(SimpleObjectName.DogFolkTail, entity.CurrentTile());
            Surgeon.SewPart(entity, tail);
        }
    }

    [Serializable]
    public class Dastard : Perk
    {
        public override PerkName PerkName => PerkName.Dastard;

        public override void OnAttack(Entity self, Entity target, EntityCommandBuffer ecb)
        {
            if (!self.GetComponentData<MoraleComponent>().isFleeing)
            {
                ecb.AddBufferElement(self, new MoraleChangeElement(-100));
                ecb.AddBufferElement(self, new EffectElement(EffectName.Haste, 100, self));
                ecb.AddBufferElement(target, new DurabilityChangeElement(-25, self, DurabilityChangeReason.Smashed));
            }
        }

        public override void Grant(Entity entity)
        {
            base.Grant(entity);
            //entity.AddBufferElement(new EffectElement(EffectName.MoraleRegen, -1, entity));
        }
    }
}