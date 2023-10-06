using System;
using System.Collections;
using UnityEngine;
using Perks;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CreateAssetMenu(fileName = "PerksDatabase", menuName = "Databases/PerksDatabase")]
public class PerksDatabase : Database<PerksDatabase, PerksTable, PerksTable.Param, Perk>, IStartUp
{
    private Dictionary<PerkName, Perk> _perksByNames;
    protected override string enumName => "PerkName";
    protected override string stringTableName => "Perks";
    private IEnumerable<Type> _perkTypes;

    public override void StartUp()
    {
        ReadPersistentList();
        _perksByNames = new Dictionary<PerkName, Perk>();
        foreach (var item in _persistentDataList)
        {
            _perksByNames.Add(item.PerkName, item);
        }
    }

    protected override void StartReimport()
    {
        base.StartReimport();
        _perkTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                     where t.IsClass && t.Namespace == "Perks"
                     select t;
    }

    protected override void ProcessParam(PerksTable.Param param)
    {
        var perkName = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<PerkName>();
        var found = false;
        foreach (var perkType in _perkTypes)
        {
            if (typeof(Perks.Perk).IsAssignableFrom(perkType) && !perkType.IsAbstract)
            {
                var perkInstance = Activator.CreateInstance(perkType) as Perks.Perk;

                if (perkInstance.PerkName == perkName)
                {
                    found = true;

                    _persistentDataList.Add(perkInstance);
                    _namesStringTableEN.AddEntry(perkName.ToString(), param.nameEn);
                    _namesStringTableRU.AddEntry(perkName.ToString(), param.nameRu);
                    _descriptionsStringTableEN.AddEntry(perkName.ToString(), param.descriptionEn);
                    _descriptionsStringTableRU.AddEntry(perkName.ToString(), param.descriptionRu);

                    break;
                }
            }
        }
        if (!found)
        {
            Debug.Log("Perk not found: " + perkName);
        }
    }

    public static Perk GetPerk(PerkName perkName)
    {
        if (instance._perksByNames.ContainsKey(perkName))
        {
            return instance._perksByNames[perkName];
        }
        else
        {
            throw new ArgumentOutOfRangeException("Perk not found: " + perkName);
        }
    }
}