using Gods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GodsDataBase", menuName = "Databases/GodsDataBase")]
public class GodsDatabase : Database<GodsDatabase, GodsTable, GodsTable.Param, God>
{
    private Dictionary<GodArchetype, God> _godsByNames;
    private SerializableDictionary<GodArchetype, bool> _archetypesUnrandomness;
    protected override string enumName => "GodArchetype";
    protected override string stringTableName => "Gods";

    public SerializableDictionary<GodArchetype, bool> ArchetypesUnrandomness { get => _archetypesUnrandomness; }

    private IEnumerable<Type> _godTypes;

    public static God[] GetAllGods()
    {
        var gods = GodsGenerator.GenerateGods();
        return gods;
    }

    public override void StartUp()
    {
        ReadPersistentList();
        _godsByNames = new Dictionary<GodArchetype, God>();
        foreach (var god in PersistentDataList)
        {
            _godsByNames.Add(god.GodArchetype, god);
        }
    }

    protected override void StartReimport()
    {
        _archetypesUnrandomness = new SerializableDictionary<GodArchetype, bool>();
        base.StartReimport();
        _godTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == "Gods"
                    select t;
    }

    protected override void ProcessParam(GodsTable.Param param)
    {
        var godArchetype = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<GodArchetype>();
        var found = false;
        _archetypesUnrandomness.Add(godArchetype, param.isUnrandom);

        foreach (var godType in _godTypes)
        {
            if (typeof(Gods.God).IsAssignableFrom(godType) && !godType.IsAbstract)
            {
                var godInstance = Activator.CreateInstance(godType) as Gods.God;
                if (godInstance.GodArchetype == godArchetype)
                {
                    found = true;

                    _persistentDataList.Add(godInstance);
                    _namesStringTableEN.AddEntry(godArchetype.ToString(), param.nameEN);
                    _namesStringTableRU.AddEntry(godArchetype.ToString(), param.nameRU);
                    _descriptionsStringTableEN.AddEntry(godArchetype.ToString(), param.descriptionEN);
                    _descriptionsStringTableRU.AddEntry(godArchetype.ToString(), param.descriptionRU);

                    break;
                }
            }
        }

        if (!found && godArchetype != GodArchetype.Null)
        {
            Debug.Log("Perk not found: " + godArchetype);
        }
    }

    public static God GetGod(GodArchetype GodArchetype)
    {
        if (instance._godsByNames.ContainsKey(GodArchetype))
        {
            return BinarySerializer.MakeDeepClone<God>(instance._godsByNames[GodArchetype]);
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("No such god " + GodArchetype);
        }
    }
}

public enum TopGodIconPart
{
    Horns1,
    Horns2,
    Horns3,
    Horns4,
    Horns5,
    Horns6,
    Horns7,
    BlindFold1,
    NoHorns,
}

public enum MiddleGodIconPart
{
    Face1,
    Face2,
    Face3,
    Face4,
    Face5,
    Face6,
    Face7,
}

public enum BottomGodIconPart
{
    Beard1,
    Beard2,
    Beard3,
    Beard4,
    Beard5,
    Beard6,
    Beard7,
    Hands1,
    NoBeard,
}

public enum Sphere
{
}

public enum GodAttentionLevel
{
    Min,
    Low,
    Medium,
    High,
    Max
}

public enum GodRelationsStatus
{
    Neutral,
    Friendly,
    Agressive,
}