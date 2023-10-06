using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleObjectsDatabase", menuName = "Databases/SimpleObjectsDatabase")]
public class SimpleObjectsDatabase : Database<SimpleObjectsDatabase, SimpleObjectsTable, SimpleObjectsTable.Param, SpawnData<SimpleObjectData>>, IStartUp
{
    private Dictionary<SimpleObjectName, ObjectType> _objectTypeByItemName;

    private Dictionary<SimpleObjectName, SimpleObjectData> _objectDataByName;

    protected override string stringTableName => "SimpleObjects";
    protected override string enumName => "SimpleObjectName";

    public static SpawnData<SimpleObjectData> GetSpawnData(SimpleObjectName simpleObjectName)
    {
        return instance._persistentDataList.FirstOrDefault(t => t.Data.SimpleObjectName == simpleObjectName);
    }

    public static SimpleObjectData GetObjectData(SimpleObjectName simpleObjectName, bool readOnly = false)
    {
        if (simpleObjectName == SimpleObjectName.Null)
        {
            return null;
        }

        if (instance._objectDataByName != null)
        {
            if (instance._objectDataByName.ContainsKey(simpleObjectName))
            {
                if (readOnly) return instance._objectDataByName[simpleObjectName];
                return instance._objectDataByName[simpleObjectName].DeepClone<SimpleObjectData>();
            }
            throw new NullReferenceException("there is no such item " + simpleObjectName);
        }
        else
        {
            foreach (var spawnData in instance._persistentDataList)
            {
                if (spawnData.Data.SimpleObjectName == simpleObjectName)
                {
                    if (readOnly) return spawnData.Data;
                    return spawnData.Data.DeepClone<SimpleObjectData>();
                }
            }
            throw new NullReferenceException("there is no such item " + simpleObjectName);
        }
    }

    public static SimpleObjectData GetRandomSimpleObject(int level, Biome biome, List<Tag> tags, bool readOnly = false)
    {
        for (int i = 0; i < instance._persistentDataList.Count; i++)
        {
            if (instance._persistentDataList[i] == null)
            {
                UnityEngine.Debug.Log(i + " is null");
            }
        }

        var data = instance._persistentDataList.GetRandomSpawnData(level, biome, tags: tags);
        if (data != null)
        {
            if (readOnly) return data.Data;
            return BinarySerializer.MakeDeepClone(data.Data);
        }
        else
        {
            var tagString = "";
            foreach (var tag in tags)
            {
                tagString += tag.ToString() + ", ";
            }

            if (tagString.Length == 0) tagString = "Any";
            UnityEngine.Debug.Log("There is no items with such criteria: " +
                                             "level " + level +
                                             ", biome " + biome +
                                             ", tags " + tagString);

            return null;

            throw new NullReferenceException();
        }
    }

    public static List<SimpleObjectData> GetItemsOfWorth(int targetWorth, int level)
    {
        var currentWorth = 0f;
        var itemsOfWorth = new List<SimpleObjectData>();
        var affordableItems = instance._persistentDataList.Where(s => s.Data.worth <= targetWorth && s.Data.worth > 0 && s.BaseSpawnChance > 0).ToList();

        if (affordableItems.Count > 0)
        {
            while (currentWorth < targetWorth)
            {
                var newItem = affordableItems.RandomItem();
                currentWorth += newItem.Data.worth;
                itemsOfWorth.Add(newItem.Data);
            }
        }

        return itemsOfWorth;
    }

    public override void StartUp()
    {
        _objectTypeByItemName = new Dictionary<SimpleObjectName, ObjectType>();
        _objectDataByName = new Dictionary<SimpleObjectName, SimpleObjectData>();

        ReadPersistentList();

        foreach (var spawnData in instance._persistentDataList)
        {
            _objectDataByName.Add(spawnData.Data.SimpleObjectName, spawnData.Data);
            _objectTypeByItemName.Add(spawnData.Data.SimpleObjectName, spawnData.Data.objectTypeComponent.objectType);
        }
    }

    protected override void ProcessParam(SimpleObjectsTable.Param param)
    {
        var simpleObjectName = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>();
        var data = new SimpleObjectData(simpleObjectName);
        var objectType = param.objectType.DecodeCharSeparatedEnumsAndGetFirst<ObjectType>();
        var tags = param.tags.DecodeCharSeparatedEnums<Tag>();

        _namesStringTableEN.AddEntry(simpleObjectName.ToString(), param.realNameEN);
        _namesStringTableRU.AddEntry(simpleObjectName.ToString(), param.realNameRU);
        _descriptionsStringTableEN.AddEntry(simpleObjectName.ToString(), param.descriptionEN);
        _descriptionsStringTableRU.AddEntry(simpleObjectName.ToString(), param.descriptionRU);

        data.defaultTileState = param.defaultState.DecodeCharSeparatedEnumsAndGetFirst<TemplateState>();
        data.sortingLayer = param.SortingLayer.DecodeCharSeparatedEnumsAndGetFirst<SortingLayer>();

        data.objectTypeComponent = new ObjectTypeComponent(objectType);
        data.objectSoundData = new ComponentReferece<ObjectSoundData>(new ObjectSoundData(param));
        data.rndSpriteNum = -1;

        if (tags.Length > 0)
        {
            foreach (var item in tags)
            {
                data.tagElements.Add(new TagBufferElement(item));
            }
        }
        if (tags.Contains(Tag.Decayable))
        {
            data.decayComponent = new ComponentReferece<DecayComponent>(new DecayComponent(DecaySystem.baseDecayTime));
        }
        if (tags.Contains(Tag.Food) || tags.Contains(Tag.Butcherable))
        {
            data.eatableComponent = new ComponentReferece<EatableComponent>(new EatableComponent(param));
        }
        if (objectType == ObjectType.Solid || objectType == ObjectType.Drop)
        {
            data.physicsComponent = new ComponentReferece<PhysicsComponent>(new PhysicsComponent(param));
            data.durabilityComponent = new ComponentReferece<DurabilityComponent>(new DurabilityComponent(param));
            data.bodyPartComponent = new ComponentReferece<BodyPartComponent>(new BodyPartComponent(param));

            if (tags.Contains(Tag.Door))
            {
                data.doorComponent = new ComponentReferece<DoorComponent>(new DoorComponent(false));
            }
            if (tags.Contains(Tag.RangedWeapon))
            {
                data.rangedWeaponComponent = new ComponentReferece<RangedWeaponComponent>(new RangedWeaponComponent(param));
            }
            if (param.internalLiquid != "")
            {
                data.internalLiquidComponent = new ComponentReferece<InternalLiquidComponent>(new InternalLiquidComponent(param));
            }
            if (param.effectsOnConsumption != "")
            {
                foreach (var effect in param.effectsOnConsumption.DecodeCharSeparatedEnums<EffectName>())
                {
                    data.effectsOnHit.Add(new EffectOnHitElement(effect));
                }
            }
            if (param.effectsOnApplying != "")
            {
                foreach (var effect in param.effectsOnApplying.DecodeCharSeparatedEnums<EffectName>())
                {
                    data.effectsOnApplying.Add(new EffectOnApplyingElement(effect));
                }
            }
            if (param.permanentEffects != "")
            {
                foreach (var effect in param.permanentEffects.DecodeCharSeparatedEnums<EffectName>())
                {
                    data.activeEffects.Add(new EffectElement(effect, -1, Entity.Null));
                }
            }
            if (!string.IsNullOrEmpty(param.drop))
            {
                var drops = param.drop.Split(',');

                foreach (var dropString in drops)
                {
                    var nameAndChance = dropString.Split('*');
                    string dropName = nameAndChance[0];
                    float chance = float.Parse(nameAndChance[1]);
                    for (float i = chance; i > 0; i -= 100)
                    {
                        var itemName = dropName.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>();
                        var dropData = new DropElement(itemName, math.min(i, 100));
                        data.dropElements.Add(dropData);
                    }
                }
            }
        }

        var biomeFilter = new EnumFilter<Biome>(param.biome);
        var spawnData = new SpawnData<SimpleObjectData>(data, biomeFilter, param.normalDepthStart, param.normalDepthEnd, param.baseSpawnChance, 1, tileMapId: param.tileMapId);
        _persistentDataList.Add(spawnData);
    }
}