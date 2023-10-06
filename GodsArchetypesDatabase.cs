using Gods;
using Gods.GodBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

[CreateAssetMenu(fileName = "ArchetypesDatabase", menuName = "Databases/ArchetypesDatabase")]
public class GodsArchetypesDatabase : Database<GodsArchetypesDatabase, GodsTable, GodsTable.Param, GodArchetypeData>
{
    [SerializeField] public SerializableDictionary<GodArchetype, GodArchetypeData> _archetypesByNames = new SerializableDictionary<GodArchetype, GodArchetypeData>();
    public SerializableDictionary<GodArchetype, GodArchetypeData> ArchetypesByNames => _archetypesByNames;
    protected override string enumName => "GodArchetype";
    protected override string stringTableName => "Gods";

    protected override void StartReimport()
    {
        base.StartReimport();
        _archetypesByNames = new SerializableDictionary<GodArchetype, GodArchetypeData>();
    }

    protected override void ProcessParam(GodsTable.Param param)
    {
        if (!param.isUnrandom)
        {
            var data = new GodArchetypeData();
            var archetype = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<GodArchetype>();
            data.GodArchetype = archetype;
            data.sacrificeTags = param.sacrificeTags.DecodeCharSeparatedEnums<Tag>();
            data.sacrificeWays = param.sacrificeWays.DecodeCharSeparatedEnums<DurabilityChangeReason>();

            data.tops = param.tops.DecodeCharSeparatedEnums<TopGodIconPart>();
            data.mids = param.mids.DecodeCharSeparatedEnums<MiddleGodIconPart>();
            data.bots = param.bots.DecodeCharSeparatedEnums<BottomGodIconPart>();

            var colorNames = param.colors.Split(',');
            data.colors = new UnityEngine.Color[colorNames.Length];
            for (int i = 0; i < colorNames.Length; i++)
            {
                if (colorNames[i].Length == 0) continue;
                var color = Color.white;
                if (ColorUtility.TryParseHtmlString("#" + colorNames[i], out color))
                {
                    data.colors[i] = color;
                }
                else
                {
                    throw new Exception("Can not parse color " + colorNames[i] + " in " + param.enumName);
                }
            }
            var lServants = param.lesserServants.DecodeCharSeparatedEnums<ComplexObjectName>();
            var gServants = param.lesserServants.DecodeCharSeparatedEnums<ComplexObjectName>();
            var incarnate = param.incarnate.DecodeCharSeparatedEnumsAndGetFirst<ComplexObjectName>();
            data.GodIncarnationData = new GodIncarnationData(incarnate, lServants, gServants);

            var pBehs = GetGodBehaviours(param.prayerBehaviours);
            var gBehs = GetGodBehaviours(param.behaviours);
            var eBehs = GetGodBehaviours("");

            data.GodBehavioursData = new GodBehavioursData(pBehs, gBehs, eBehs);

            var perksTiers = new GodPercsData.PerksTier[10];
            for (int i = 0; i < 10; i++)
            {
                var index = (i + 1) * 10;
                var perks = (param.GetType().GetField("perks" + index).GetValue(param) as string).DecodeCharSeparatedEnums<PerkName>().ToArray();
                perksTiers[i] = new GodPercsData.PerksTier(perks);
            }
            data.GodPercsData = new GodPercsData(perksTiers);

            _descriptionsStringTableEN.AddEntry(param.enumName, param.descriptionEN);
            _descriptionsStringTableRU.AddEntry(param.enumName, param.descriptionRU);

            _archetypesByNames.Add(archetype, data);
            Debug.Log(_archetypesByNames[archetype].GodBehavioursData.PrayerBehaviours.Length);
        }
    }

    public GodBehaviourName[] GetGodBehaviours(string str)
    {
        return str.DecodeCharSeparatedEnums<GodBehaviourName>();
    }

    public GodArchetypeData GetGodArchetypeData(GodArchetype godArchetype)
    {
        return _archetypesByNames[godArchetype];
    }

    public override void StartUp()
    {
    }
}