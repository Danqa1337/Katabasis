using Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "EffectsDatabase", menuName = "Databases/EffectsDatabase")]
public class EffectsDatabase : Database<EffectsDatabase, EffectsTable, EffectsTable.Param, Effect>
{
    private Dictionary<EffectName, Effect> _effectsByNames;
    protected override string enumName => "EffectName";
    protected override string stringTableName => "Effects";
    private IEnumerable<Type> _effectTypes;

    public override void StartUp()
    {
        ReadPersistentList();
        _effectsByNames = new Dictionary<EffectName, Effect>();
        foreach (var item in _persistentDataList)
        {
            _effectsByNames.Add(item.EffectName, item);
        }
    }

    public static Effect GetEffect(EffectName effectName)
    {
        if (instance._effectsByNames.ContainsKey(effectName))
        {
            return BinarySerializer.MakeDeepClone<Effect>(instance._effectsByNames[effectName]);
        }
        else
        {
            throw new Exception("Effect not found: " + effectName);
        }
    }

    protected override void StartReimport()
    {
        base.StartReimport();
        _effectTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                       where t.IsClass && t.Namespace == "Effects"
                       select t;
    }

    protected override void ProcessParam(EffectsTable.Param param)
    {
        var effectName = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<EffectName>();
        var found = false;
        foreach (var effectType in _effectTypes)
        {
            if (typeof(Effects.Effect).IsAssignableFrom(effectType) && !effectType.IsAbstract)
            {
                var effectInstance = Activator.CreateInstance(effectType) as Effects.Effect;

                if (effectInstance.EffectName == effectName)
                {
                    found = true;

                    _persistentDataList.Add(effectInstance);
                    _namesStringTableEN.AddEntry(effectName.ToString(), param.nameEN);
                    _namesStringTableRU.AddEntry(effectName.ToString(), param.nameRU);
                    _descriptionsStringTableEN.AddEntry(effectName.ToString(), param.descriptionEN);
                    _descriptionsStringTableRU.AddEntry(effectName.ToString(), param.descriptionRU);

                    break;
                }
            }
        }
        if (!found)
        {
            Debug.Log("Effect not found: " + effectName);
        }
    }
}