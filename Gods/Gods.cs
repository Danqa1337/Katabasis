using System.Collections;
using UnityEngine;
using System;
using Gods.GodBehaviours;

namespace Gods
{
    [Serializable]
    public class Armok : God
    {
        public override GodArchetype GodArchetype => GodArchetype.Armok;
    }

    [Serializable]
    public class Pushun : God
    {
        public override GodArchetype GodArchetype => GodArchetype.Pushun;

        public override bool CanAcceptSacrifice(SacrificeData sacrificeData)
        {
            return sacrificeData.Entity.HasTag(Tag.Food);
        }

        public override int GetSacrificeWorth(SacrificeData sacrificeData)
        {
            return base.GetSacrificeWorth(sacrificeData) * 2;
        }
    }

    [Serializable]
    public class DarkHelios : God
    {
        public override GodArchetype GodArchetype => GodArchetype.DarkHelios;
    }

    [Serializable]
    public class Ea : God
    {
        public override GodArchetype GodArchetype => GodArchetype.Ea;
    }

    [Serializable]
    public class Acephalos : God
    {
        public override GodArchetype GodArchetype => GodArchetype.Acephalos;
    }

    [Serializable]
    public class CzokCzok : God
    {
        public override GodArchetype GodArchetype => GodArchetype.CzokCzok;

        public override bool CanAcceptSacrifice(SacrificeData sacrificeData)
        {
            return sacrificeData.Entity.HasTag(Tag.Weapon);
        }

        public override int GetSacrificeWorth(SacrificeData sacrificeData)
        {
            return base.GetSacrificeWorth(sacrificeData) * 2;
        }
    }
}