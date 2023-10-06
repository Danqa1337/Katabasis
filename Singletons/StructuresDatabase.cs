using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StructuresDatabase", menuName = "Databases/StructuresDatabase")]
public class StructuresDatabase : Database<StructuresDatabase, StructuresTable, StructuresTable.Param, SpawnData<StructureData>>
{
    protected override string enumName => "StructureName";
    private static Dictionary<StructureName, StructureData> _structuresByName;
    private static Dictionary<HyperstuctureName, Hyperstructure> _hyperstructuresByName;

    public override void StartUp()
    {
        ReadPersistentList();
        _structuresByName = new Dictionary<StructureName, StructureData>();
        _hyperstructuresByName = new Dictionary<HyperstuctureName, Hyperstructure>();
        foreach (var item in instance._persistentDataList)
        {
            if (item.Data != null)
            {
                _structuresByName.Add(item.Data.StructureName, item.Data);
            }
            else
            {
                Debug.Log("structure is null");
            }
        }

        _hyperstructuresByName.Add(HyperstuctureName.Town, new TownHyperstucture());
    }

    public static StructureData GetStructureData(StructureName structureName)
    {
        if (_structuresByName.ContainsKey(structureName))
        {
            return _structuresByName[structureName];
        }
        throw new System.NullReferenceException("No such structure " + structureName);
    }

    public static Hyperstructure GetHyperstructure(HyperstuctureName hyperstuctureName)
    {
        if (_hyperstructuresByName.ContainsKey(hyperstuctureName))
        {
            return _hyperstructuresByName[hyperstuctureName];
        }
        throw new System.NullReferenceException("No such hyperstructure " + hyperstuctureName);
    }

    public static StructureData GetRandomStructureData(GenerationPresetName generationPreset, int level, int maxSize = 999)
    {
        var structuresOfThatSize = instance._persistentDataList.Where(d => d.Data.GetMaxExtent() <= maxSize);
        var data = structuresOfThatSize.GetRandomSpawnData(level, generationPresetName: generationPreset).Data;
        return data;
    }

    protected override void ProcessParam(StructuresTable.Param param)
    {
        var structureName = param.enumName.DecodeCharSeparatedEnumsAndGetFirst<StructureName>();
        var structureData = new StructureData(structureName);

        var itemNamesByTileMapId = new Dictionary<int, SimpleObjectName>();
        SimpleObjectsDatabase.PersistentDataList.ForEach(d =>
        {
            if (d.TileMapId != -1)
            {
                itemNamesByTileMapId.Add(d.TileMapId, d.Data.SimpleObjectName);
            }
        });

        var structureMap = TiledStructureImporter.LoadStructure(structureName, itemNamesByTileMapId);

        if (structureMap != null)
        {
            var defaultBiome = param.DefaultBiome.DecodeCharSeparatedEnumsAndGetFirst<Biome>();
            var creatureNames = new ComplexObjectName[10];

            var spawnPointsParams = new string[10]
            {
                param.CreatureSpawnPoint0,
                param.CreatureSpawnPoint1,
                param.CreatureSpawnPoint2,
                param.CreatureSpawnPoint3,
                param.CreatureSpawnPoint4,
                param.CreatureSpawnPoint5,
                param.CreatureSpawnPoint6,
                param.CreatureSpawnPoint7,
                param.CreatureSpawnPoint8,
                param.CreatureSpawnPoint9,
            };

            for (int i = 0; i < 10; i++)
            {
                if (spawnPointsParams[i] != "" && spawnPointsParams[i] != "Null")
                {
                    creatureNames[i] = spawnPointsParams[i].DecodeCharSeparatedEnumsAndGetFirst<ComplexObjectName>();
                }
            }
            for (int i = 0; i < structureMap.Map.Length; i++)
            {
                var template = structureMap.Map[i];
                template.Biome = defaultBiome;
                structureMap.SetTemplate(template, i);
            }

            structureData.TemplateStateFilter = new EnumFilter<TemplateState>(param.OverlapingTemplateState);
            structureData.structureMap = structureMap;
            structureData.CreatureNames = creatureNames;

            _persistentDataList.Add(new SpawnData<StructureData>(structureData,
                param.normalDepthStart,
                param.normalDepthEnd,
                param.baseSpawnChance,
                biomeFilter: new EnumFilter<Biome>(param.AllowedBiomes),
                presetFilter: new EnumFilter<GenerationPresetName>(param.AllowedPresets),
                active: param.Enabled));
            Debug.Log(_persistentDataList[_persistentDataList.Count - 1].Data == null);
        }
    }
}