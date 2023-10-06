using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;
using Assets.Scripts;
using Gods.GodBehaviours;
using static RandomizedGod;

namespace Gods
{
    [Serializable]
    public abstract class God
    {
        public virtual string Name { get => GodArchetype.ToString(); }
        public abstract GodArchetype GodArchetype { get; }
        public virtual int Index { get => (int)(GodArchetype); }

        private int _relations;
        private int _attention;

        protected GodsPreferences _preferences;
        protected GodIncarnationData _godIncarnationData;
        protected GodPercsData _godPercsData;
        protected GodBehavioursData _godBehavioursData;
        public int Relations => _relations;
        public int Attention => _attention;
        public GodsPreferences Preferences => _preferences;
        public GodPercsData GodPercsData => _godPercsData;
        public GodIncarnationData GodIncarnationData => _godIncarnationData;
        public GodBehavioursData GodBehavioursData => _godBehavioursData;

        public event Action OnRelationsChanged;

        public event Action OnAttentionChanged;

        public GodAttentionLevel AttentionLevel
        {
            get
            {
                if (_attention > 90) return GodAttentionLevel.Max;
                if (_attention > 75) return GodAttentionLevel.High;
                if (_attention > 50) return GodAttentionLevel.Medium;
                if (_attention > 25) return GodAttentionLevel.Low;
                return GodAttentionLevel.Min;
            }
        }

        public GodRelationsStatus RelationsStatus
        {
            get
            {
                if (_relations < 0) return GodRelationsStatus.Agressive;
                if (_relations > 0) return GodRelationsStatus.Friendly;
                return GodRelationsStatus.Neutral;
            }
        }

        public virtual bool CanAcceptSacrifice(SacrificeData sacrificeData)
        {
            return false;
        }

        public virtual int GetSacrificeWorth(SacrificeData sacrificeData)
        {
            return sacrificeData.Entity.GetComponentData<PhysicsComponent>().worth;
        }

        public virtual void OnTick()
        {
            if (KatabasisUtillsClass.Chance(1 * Attention / 100))
            {
                switch (RelationsStatus)
                {
                    case GodRelationsStatus.Neutral:
                        break;

                    case GodRelationsStatus.Friendly:
                        DoGoodAction();
                        break;

                    case GodRelationsStatus.Agressive:
                        DoEvilAction();
                        break;

                    default:
                        break;
                }
            }
        }

        public virtual void OnPray()
        {
            var bestBehaviour = _godBehavioursData.PrayerBehaviours.OrderByDescending(b => b.Evaluate(this)).FirstOrDefault();
            Debug.Log(GodArchetype + " reacts on prayer with behaviour: " + bestBehaviour);
            bestBehaviour.Execute(this);
        }

        protected virtual void DoGoodAction()
        {
            var bestBehaviour = _godBehavioursData.GoodBehaviours.OrderByDescending(b => b.Evaluate(this)).FirstOrDefault();
            Debug.Log(GodArchetype + " performs good action " + bestBehaviour);
            bestBehaviour.Execute(this);
        }

        protected virtual void DoEvilAction()
        {
            var bestBehaviour = _godBehavioursData.EvilBehaviours.OrderByDescending(b => b.Evaluate(this)).FirstOrDefault();
            Debug.Log(GodArchetype + " performs evil action " + bestBehaviour);
            bestBehaviour.Execute(this);
        }

        public virtual void OnSacrifice(SacrificeData sacrificeData)
        {
            var newAttention = Attention;
            if (CanAcceptSacrifice(sacrificeData))
            {
                newAttention = Math.Min(100, newAttention + UnityEngine.Random.Range(5, 20));
                var worth = GetSacrificeWorth(sacrificeData);
                Debug.Log(this + " accepts " + sacrificeData);
                AddRelations(worth);
            }
            else
            {
                newAttention = Math.Min(30, newAttention + UnityEngine.Random.Range(0, 5));
                Debug.Log(this + " does not accept " + sacrificeData);
            }
            SetAttention(newAttention);
        }

        public virtual void OnPerkRequest()
        {
            List<PerkName> validPerks = new List<PerkName>();
            var maxPerkTierIndex = Math.Clamp((int)(Relations / 10) - 1, 0, 9);
            for (int i = 0; i < maxPerkTierIndex; i++)
            {
                var tier = GodPercsData.PerksTiers[i];
                if (tier.Perks.Length > 0)
                {
                    validPerks.AddRange(tier.Perks.Where(p => !PerksTree.HasPerk(Player.PlayerEntity, p)));
                }
            }
            if (validPerks.Count > 0)
            {
                PerksTree.GrantPerk(validPerks.RandomItem(), Player.PlayerEntity);
            }
            else
            {
                Debug.Log(this + " has no perks to grant");
            }
        }

        public void AddRelations(int relations)
        {
            SetRelations(_relations + relations);
        }

        public void SetRelations(int relations)
        {
            Debug.Log("Relations with " + this + " changed from " + _relations + " to " + relations);
            _relations = relations;
            OnRelationsChanged?.Invoke();
        }

        public void SetAttention(int attention)
        {
            Debug.Log("Attention of " + this + " changed from " + _attention + " to " + attention);
            _attention = attention;
            OnAttentionChanged?.Invoke();
        }

        public override string ToString()
        {
            return GodArchetype.ToString() + " " + Name;
        }
    }
}

[Serializable]
public class GodIncarnationData
{
    [SerializeField] public ComplexObjectName Incarnate;
    [SerializeField] public ComplexObjectName[] LesserServats;
    [SerializeField] public ComplexObjectName[] GreatherServats;

    public GodIncarnationData(ComplexObjectName incarnate, ComplexObjectName[] lesserServats, ComplexObjectName[] greatherServats)
    {
        Incarnate = incarnate;
        LesserServats = lesserServats;
        GreatherServats = greatherServats;
    }
}

[Serializable]
public class GodPercsData
{
    public PerksTier[] PerksTiers;

    public GodPercsData(PerksTier[] perksTiers)
    {
        PerksTiers = perksTiers;
    }

    public PerksTier GetPerksTier(int tier)
    {
        return PerksTiers[tier];
    }

    [System.Serializable]
    public class PerksTier
    {
        public PerkName[] Perks;

        public PerksTier(PerkName[] perks)
        {
            Perks = perks;
        }
    }
}