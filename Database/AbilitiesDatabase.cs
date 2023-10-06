using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "AbilitiesDatabase", menuName = "Databases/AbilitiesDatabase")]
public class AbilitiesDatabase : Database<AbilitiesDatabase, AbilitiesTable, AbilitiesTable.Param, AbilityData>, IStartUp
{
    private static Dictionary<AbilityName, AbilityData> _abilityDataByName = new Dictionary<AbilityName, AbilityData>();
    protected override string enumName => "AbilityName";
    protected override string stringTableName => "Abilities";
    private static IEnumerable<Type> _abilityTypes;

    public override void StartUp()
    {
        ReadPersistentList();
        _abilityDataByName = new Dictionary<AbilityName, AbilityData>();
        foreach (var item in instance._persistentDataList)
        {
            _abilityDataByName.Add(item.AbilityName, item);
        }
    }

    public static AbilityData GetAbilityData(AbilityName abilityName)
    {
        if (_abilityDataByName.ContainsKey(abilityName))
        {
            return _abilityDataByName[abilityName];
        }
        throw new Exception("No such ability " + abilityName);
    }

    public static AbilityData[] GetAllAbilities()
    {
        return _abilityDataByName.Values.ToArray();
    }

    protected override void StartReimport()
    {
        base.StartReimport();
        _abilityTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsClass && t.Namespace == "Abilities"
                        select t;
    }

    protected override void ProcessParam(AbilitiesTable.Param param)
    {
        var name = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<AbilityName>();
        var found = false;
        foreach (var abilityType in _abilityTypes)
        {
            if (typeof(Abilities.Ability).IsAssignableFrom(abilityType) && !abilityType.IsAbstract)
            {
                var abilityInstance = Activator.CreateInstance(abilityType) as Abilities.Ability;
                if (abilityInstance.AbilityName == name)
                {
                    var abilityData = new AbilityData(abilityInstance, param);

                    _namesStringTableEN.AddEntry(abilityInstance.AbilityName.ToString(), param.NameEn);
                    _namesStringTableRU.AddEntry(abilityInstance.AbilityName.ToString(), param.NameRu);
                    _descriptionsStringTableEN.AddEntry(abilityInstance.AbilityName.ToString(), param.DescriptionEn);
                    _descriptionsStringTableRU.AddEntry(abilityInstance.AbilityName.ToString(), param.DescriptionRu);

                    instance._persistentDataList.Add(abilityData);
                    found = true;
                    break;
                }
            }
        }

        if (!found && name != AbilityName.Null)
        {
            Debug.Log("Ability not found: " + name);
        }
    }
}

[Serializable]
public class AbilityData
{
    public readonly AbilityName AbilityName;
    [SerializeReference] public readonly Abilities.Ability Ability;
    public readonly Abilities.AbilityTargeting AbilityTargeting;
    public readonly int BaseCooldown;
    public readonly int GnosisCost;
    public readonly float Power;
    public readonly bool ShowEquipInAim;
    public readonly bool ShowColorsInAim;
    public readonly bool DisplayInHotbar;

    public AbilityData(Abilities.Ability ability, AbilitiesTable.Param param)
    {
        AbilityName = ability.AbilityName;
        Ability = ability;
        BaseCooldown = param.Cooldown;
        AbilityTargeting = param.TargetType.DecodeCharSeparatedEnumsAndGetFirst<Abilities.AbilityTargeting>();
        GnosisCost = param.GnosisCost;
        Power = param.Power;
        ShowEquipInAim = param.ShowEquipOnAim;
        ShowColorsInAim = param.AimColorMode != "";
        DisplayInHotbar = param.displayInHotbar;
    }
}