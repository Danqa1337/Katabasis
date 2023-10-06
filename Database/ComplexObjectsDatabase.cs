using System.Collections.Generic;
using UnityEngine;
using System.Security.Policy;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "ComplexObjectsDatabase", menuName = "Databases/ComplexObjectsDatabase")]
public class ComplexObjectsDatabase : Database<ComplexObjectsDatabase, ComplexObjectsTable, ComplexObjectsTable.Param, SpawnData<ComplexObjectData>>, IStartUp
{
    [SerializeField] private Dictionary<ComplexObjectName, ComplexObjectData> _complexObjectsByNames;
    protected override string enumName => "ComplexObjectName";
    protected override string stringTableName => "ComplexObjects";

    public static ComplexObjectData GetObjectData(ComplexObjectName complexObjectName, bool readOnly = false)
    {
        if (Application.isPlaying)
        {
            if (instance._complexObjectsByNames.ContainsKey(complexObjectName))
            {
                if (readOnly)
                {
                    return instance._complexObjectsByNames[complexObjectName];
                }
                else
                {
                    return instance._complexObjectsByNames[complexObjectName].DeepClone<ComplexObjectData>();
                }
            }
            else
            {
                throw new System.Exception("No such ComplexObject " + complexObjectName);
            }
        }
        else
        {
            if (instance._persistentDataList.Any(s => s.Data.ComplexObjectName == complexObjectName))
            {
                var data = instance._persistentDataList.First(t => t.Data.ComplexObjectName == complexObjectName).Data;
                if (readOnly) return data;
                return data.DeepClone<ComplexObjectData>();
            }
            else
            {
                throw new Exception("can not find " + complexObjectName + " in persistent list");
            }
        }
    }

    public override void StartUp()
    {
        ReadPersistentList();
        _complexObjectsByNames = new Dictionary<ComplexObjectName, ComplexObjectData>();
        foreach (var item in _persistentDataList)
        {
            _complexObjectsByNames.Add(item.Data.ComplexObjectName, item.Data);
        }
    }

    protected override void ProcessParam(ComplexObjectsTable.Param param)
    {
        var complexObjectName = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<ComplexObjectName>();
        var data = new ComplexObjectData(complexObjectName);
        var tags = param.tags.DecodeCharSeparatedEnums<Tag>();
        var defaultAbilities = new AbilityName[]
        {
            AbilityName.SpendTime1,
            AbilityName.SpendTime10,
            AbilityName.PicUp,
            AbilityName.Drop,
            AbilityName.Eat,
            AbilityName.ReloadWeapon,
            AbilityName.ToggleMechanism,
        };

        _namesStringTableEN.AddEntry(complexObjectName.ToString(), param.realNameEN);
        _namesStringTableRU.AddEntry(complexObjectName.ToString(), param.realNameRU);
        _descriptionsStringTableEN.AddEntry(complexObjectName.ToString(), param.descriptionEN);
        _descriptionsStringTableRU.AddEntry(complexObjectName.ToString(), param.descriptionRU);

        if (tags.Contains(Tag.Creature))
        {
            data.alive = true;
        }

        if (param.properties == "")
        {
            data.creatureComponent = new ComponentReferece<CreatureComponent>(new CreatureComponent(param));
            data.equipmentComponent = new ComponentReferece<EquipmentComponent>(new EquipmentComponent(param));
            data.moraleComponent = new ComponentReferece<MoraleComponent>(new MoraleComponent(100));

            if (param.enemyTags != "")
            {
                data.enemyTagElements = new List<EnemyTagBufferElement>();
                foreach (var item in param.enemyTags.DecodeCharSeparatedEnums<Tag>())
                {
                    data.enemyTagElements.Add(new EnemyTagBufferElement(item));
                }
            }

            if (param.Abilities != "")
            {
                foreach (var item in param.Abilities.DecodeCharSeparatedEnums<AbilityName>())
                {
                    data.availableAbilities.Add(new AbilityElement(item));
                }
                foreach (var item in defaultAbilities)
                {
                    data.availableAbilities.Add(new AbilityElement(item));
                }
            }

            foreach (var bodypartTag in param.bodyparts.DecodeCharSeparatedEnums<BodyPartTag>())
            {
                var bodyPartData = SimpleObjectsDatabase.GetObjectData((param.enumName + bodypartTag.ToString()).DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>());

                var field = data.GetType().GetField(bodypartTag.ToString());
                if (field == null)
                {
                    throw new NullReferenceException(bodypartTag.ToString());
                }
                field.SetValue(data, bodyPartData);
            }
            if (data.Body.objectTypeComponent.objectType == ObjectType.Drop)
            {
                data.Body.objectTypeComponent = new ObjectTypeComponent(ObjectType.Solid);
            }
            foreach (var item in tags)
            {
                data.Body.tagElements.Add(new TagBufferElement(item));
            }
        }
        else
        {
            data = ObjectDataFactory.GetComplexObjectDataFromString(param.properties);
            data.GetType().GetField("ComplexObjectName").SetValue(data, complexObjectName);
        }

        var spawnData = new SpawnData<ComplexObjectData>(data, biomeFilter: new EnumFilter<Biome>(param.biome), normalDepthStart: param.normalDepthStart, normalDepthEnd: param.normalDepthEnd, baseSpawnChance: param.baseSpawnChance, maxPackSize: param.maxPackSize);
        _persistentDataList.Add(spawnData);
    }
}