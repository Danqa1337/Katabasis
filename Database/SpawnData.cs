using FMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class SpawnData<T> where T : DeepClonable
{
    [SerializeField] public T Data;
    public readonly int NormalDepthStart;
    public readonly int NormalDepthEnd;
    public readonly float BaseSpawnChance;
    public readonly int TileMapId;
    public readonly int MaxPackSize;
    [SerializeField] public readonly EnumFilter<Biome> BiomeFilter = new EnumFilter<Biome>();
    [SerializeField] public readonly EnumFilter<GenerationPresetName> PresetFilter = new EnumFilter<GenerationPresetName>();

    public readonly bool Active = true;

    public SpawnData(T data, EnumFilter<Biome> biomeFilter, int normalDepthStart, int normalDepthEnd, float baseSpawnChance, int maxPackSize, string tileMapId, bool active = true)
    {
        Data = data;
        BiomeFilter = biomeFilter;
        NormalDepthStart = normalDepthStart;
        NormalDepthEnd = normalDepthEnd;
        BaseSpawnChance = baseSpawnChance;
        TileMapId = tileMapId != "" ? tileMapId.ConcatToInt() : -1;
        MaxPackSize = maxPackSize;
        Active = active;
    }

    public SpawnData(T data, EnumFilter<Biome> biomeFilter, EnumFilter<GenerationPresetName> presetFilter, int normalDepthStart, int normalDepthEnd, float baseSpawnChance, int maxPackSize, bool active = true)
    {
        Data = data;
        BiomeFilter = biomeFilter;
        PresetFilter = presetFilter;
        NormalDepthStart = normalDepthStart;
        NormalDepthEnd = normalDepthEnd;
        BaseSpawnChance = baseSpawnChance;
        MaxPackSize = maxPackSize;
        Active = active;
    }

    public SpawnData(T data, EnumFilter<Biome> biomeFilter, int normalDepthStart, int normalDepthEnd, float baseSpawnChance, bool active = true)
    {
        Data = data;
        BiomeFilter = biomeFilter;
        NormalDepthStart = normalDepthStart;
        NormalDepthEnd = normalDepthEnd;
        BaseSpawnChance = baseSpawnChance;
        Active = active;
    }

    public SpawnData(T data, int normalDepthStart, int normalDepthEnd, float baseSpawnChance, int tileMapId = 0, int maxPackSize = 0, EnumFilter<Biome> biomeFilter = null, EnumFilter<GenerationPresetName> presetFilter = null, bool active = true)
    {
        Data = data;
        NormalDepthStart = normalDepthStart;
        NormalDepthEnd = normalDepthEnd;
        BaseSpawnChance = baseSpawnChance;
        TileMapId = tileMapId;
        MaxPackSize = maxPackSize;
        BiomeFilter = biomeFilter;
        PresetFilter = presetFilter;
        Active = active;
    }
}

public static class SpawnDataExtensions
{
    public static SpawnData<T> GetRandomSpawnData<T>(this IEnumerable<SpawnData<T>> collection, int level, Biome biome = Biome.Any, GenerationPresetName generationPresetName = GenerationPresetName.Any, List<Tag> tags = null, bool readOnly = false) where T : DeepClonable
    {
        var properItemsList = new List<SpawnData<T>>();
        foreach (var item in collection)
        {
            if (item.PresetFilter == null) throw new Exception("preset filter is null");
            if (item.BiomeFilter == null) throw new Exception("biome filter is null");
            bool isProperItem = item.BaseSpawnChance > 0 && item.BiomeFilter.Allows(biome) && item.PresetFilter.Allows(generationPresetName);

            if (isProperItem) properItemsList.Add(item);
        }

        return properItemsList.GetRandomSpawnData(level);
    }

    public static SpawnData<T> GetRandomSpawnData<T>(this IEnumerable<SpawnData<T>> collection, int level) where T : DeepClonable
    {
        float sum = 0;
        float iterator = 0;
        var array = collection.ToArray();
        var dataOfThatLevel = new HashSet<SpawnData<T>>();

        for (var index = 0; index < array.Length; index++)
        {
            var spawnData = array[index];
            float start = spawnData.NormalDepthStart;
            float end = spawnData.NormalDepthEnd;
            start = math.min(KatabasisUtillsClass.GenerateNormalRandom(start, LowLevelSettings.instance.spawnLevelDeviation), start);
            end = math.max(KatabasisUtillsClass.GenerateNormalRandom(end, LowLevelSettings.instance.spawnLevelDeviation), end);
            if (level >= start && level <= end)
            {
                sum += spawnData.BaseSpawnChance;
                dataOfThatLevel.Add(spawnData);
            }
        }

        float rnd = UnityEngine.Random.Range(0.001f, sum);
        foreach (var item in dataOfThatLevel)
        {
            iterator += item.BaseSpawnChance;
            if (rnd <= iterator) return item;
        }
        return null;
    }
}