using System.Collections;
using System;
using Unity.Entities;

namespace Perks
{
    [Serializable]
    public abstract class AbilityGrantingPerk : Perk
    {
        public abstract AbilityName AbilityName { get; }

        public override void Grant(Entity entity)
        {
            base.Grant(entity);

            if (entity.HasBuffer<AbilityElement>())
            {
                entity.AddBufferElement(new AbilityElement(AbilityName));
            }
            else
            {
                throw new System.Exception("There is no ability buffer on " + entity.GetName());
            }
        }

        public override void Revoke(Entity entity)
        {
            base.Revoke(entity);

            if (entity.HasBuffer<AbilityElement>())
            {
                entity.GetBuffer<AbilityElement>().Remove(new AbilityElement(AbilityName));
            }
            else
            {
                throw new System.Exception("There is no ability buffer on " + entity.GetName());
            }
        }
    }

    [Serializable]
    public class Amputate : AbilityGrantingPerk
    {
        public override PerkName PerkName => PerkName.Amputate;

        public override AbilityName AbilityName => AbilityName.Amputate;
    }

    [Serializable]
    public class Dinging : AbilityGrantingPerk
    {
        public override AbilityName AbilityName => AbilityName.Dig;

        public override PerkName PerkName => PerkName.Digging;
    }

    [Serializable]
    public class HealOther : AbilityGrantingPerk
    {
        public override AbilityName AbilityName => AbilityName.HealOther;

        public override PerkName PerkName => PerkName.HealOther;
    }

    [Serializable]
    public class HealSelf : AbilityGrantingPerk
    {
        public override AbilityName AbilityName => AbilityName.HealSelf;

        public override PerkName PerkName => PerkName.HealSelf;
    }

    [Serializable]
    public class Blink : AbilityGrantingPerk
    {
        public override AbilityName AbilityName => AbilityName.Blink;

        public override PerkName PerkName => PerkName.Blink;
    }

    [Serializable]
    public class HeadButt : AbilityGrantingPerk
    {
        public override AbilityName AbilityName => AbilityName.HeadButt;

        public override PerkName PerkName => PerkName.HeadButt;
    }

    [Serializable]
    public class SelfHarm : AbilityGrantingPerk
    {
        public override AbilityName AbilityName => AbilityName.SelfHarm;

        public override PerkName PerkName => PerkName.SelfHarm;
    }
}