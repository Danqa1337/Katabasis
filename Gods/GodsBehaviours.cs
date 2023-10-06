using Assets.Scripts;
using Gods.GodBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[Serializable]
public class GodBehavioursData
{
    [SerializeField] public GodBehaviour[] PrayerBehaviours => InitBehaviours(PrayerBehaviourNames);
    [SerializeField] public GodBehaviour[] GoodBehaviours => InitBehaviours(GoodBehaviourNames);
    [SerializeField] public GodBehaviour[] EvilBehaviours => InitBehaviours(EvilBehaviourNames);

    public GodBehaviourName[] PrayerBehaviourNames;
    public GodBehaviourName[] GoodBehaviourNames;
    public GodBehaviourName[] EvilBehaviourNames;

    public GodBehavioursData(GodBehaviourName[] prayerBehaviourNames, GodBehaviourName[] goodBehaviourNames, GodBehaviourName[] badBehaviourNames)
    {
        PrayerBehaviourNames = prayerBehaviourNames;
        GoodBehaviourNames = goodBehaviourNames;
        EvilBehaviourNames = badBehaviourNames;
    }

    private GodBehaviour[] InitBehaviours(GodBehaviourName[] names)
    {
        var result = new GodBehaviour[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            var behaviour = Activator.CreateInstance("Assembly-Csharp", "Gods.GodBehaviours." + names[i]).Unwrap() as GodBehaviour;
            result[i] = behaviour;
        }
        return result;
    }
}

namespace Gods.GodBehaviours
{
    public enum GodBehaviourName
    {
        None,
        RestorePart,
        Incarnate,
        GrantPerk,
        SpawnLesserServants,
        SpawnGreatherServants,
        ProduceCreepySounds,
    }

    [Serializable]
    public class GodBehaviour
    {
        public virtual float Evaluate(God god)
        {
            return 0;
        }

        public virtual void Execute(God god)
        {
            Debug.Log("null behaviour");
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }

    [Serializable]
    public class None : GodBehaviour
    {
        public override float Evaluate(God god)
        {
            return 0;
        }

        public override void Execute(God god)
        {
        }
    }

    [Serializable]
    public class RestorePart : GodBehaviour
    {
        public override float Evaluate(God god)
        {
            if (god.AttentionLevel >= GodAttentionLevel.Medium)
            {
                if (Player.PlayerEntity.GetBuffer<MissingBodypartBufferElement>().Length > 0)
                {
                    return 100;
                }
            }
            return -1;
        }

        public override void Execute(God god)
        {
            var bodyPart = Player.PlayerEntity.GetBuffer<MissingBodypartBufferElement>()[0].tag;
            Surgeon.RestorePart(Player.PlayerEntity, bodyPart);
        }
    }

    [Serializable]
    public class Incarnate : GodBehaviour
    {
        public override float Evaluate(God god)
        {
            if (god.GodIncarnationData.Incarnate != ComplexObjectName.Null)
            {
                return 100;
            }
            return -1;
        }

        public override void Execute(God god)
        {
            var tiles = Player.CurrentTile.GetTilesInRadius(3).ToList();
            if (tiles.Count > 0)
            {
                Spawner.Spawn(god.GodIncarnationData.Incarnate, tiles.RandomItem());
            }
            else
            {
                Debug.Log("No place to incarnate");
            }
        }
    }

    [Serializable]
    public class GrantPerk : GodBehaviour
    {
        public override float Evaluate(God god)
        {
            if (god.Relations > 0)
            {
                return 1;
            }
            return -1;
        }

        public override void Execute(God god)
        {
            god.OnPerkRequest();
        }
    }

    [Serializable]
    public abstract class SpawnServants : GodBehaviour
    {
        public virtual List<Entity> Spawn(ComplexObjectName[] objects, int number)
        {
            if (objects.Length > 0)
            {
                var pack = new PackData<ComplexObjectData>(PackName.Any);

                for (int i = 0; i < number; i++)
                {
                    pack.members.Add(ComplexObjectsDatabase.GetObjectData(objects.RandomItem()));
                }

                return Spawner.SpawnPack(pack, Player.CurrentTile);
            }
            return new List<Entity>();
        }
    }

    [Serializable]
    public class SpawnLesserServants : SpawnServants
    {
        public override float Evaluate(God god)
        {
            if (god.GodIncarnationData.LesserServats.Length > 0)
            {
                return 1;
            }
            return 0;
        }

        public override void Execute(God god)
        {
            var numberOfServants = UnityEngine.Random.Range(1, Mathf.Max(1, god.Attention));

            Spawn(god.GodIncarnationData.LesserServats, numberOfServants);
        }
    }

    [Serializable]
    public class SpawnGreatherServants : SpawnServants
    {
        public override float Evaluate(God god)
        {
            if (god.GodIncarnationData.GreatherServats.Length > 0)
            {
                return 1;
            }
            return 0;
        }

        public override void Execute(God god)
        {
            var numberOfServants = UnityEngine.Random.Range(1, Mathf.Max(1, god.Attention));

            Spawn(god.GodIncarnationData.GreatherServats, numberOfServants);
        }
    }

    [Serializable]
    public class ProduceCreepySounds : GodBehaviour
    {
        public override float Evaluate(God god)
        {
            return KatabasisUtillsClass.Chance(5 * god.Attention) ? 1 : 0.5f;
        }

        public override void Execute(God god)
        {
            Debug.Log("Creepy Sounds");
        }
    }
}