using System;
using System.Diagnostics;
using Unity.Entities;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Locations;
using static LowLevelSettings;
using System.Collections.Generic;
using UnityEditor.Localization.Editor;
using UnityEngine.Localization.Settings;

public enum Biome
{
    Dungeon,
    Cave,
    SmallCave,
    FloodedCave,
    Chasm,
    Lake,
    HotCave,
    Arena,
    TestPreset,
    Pit,
    Any,
    Entrance,
    LakeArena,
    Wall,
    LakeShrine,
    Town,
}

public enum GenerationPresetName
{
    Null,
    Any,
    Dungeon,
    Lake,
    Pit,
    Arena,
    BigChasm,
    River,
}

public class LocationGenerator : Singleton<LocationGenerator>
{
    [SerializeField] private int _seed;
    [SerializeField] private bool _alwaysRandomSeed;
    [SerializeField] private bool _debugStairs;
    [SerializeField] private Location _testLocation;

    public static event Action<Location> OnLocationGenerationStarted;

    public static event Action<Location> OnLocationGenerationComplete;

    public void Generate(Location location, Transition transition)
    {
        Debug.Log("Generating " + location);
        var globalStopWatch = new Stopwatch();
        globalStopWatch.Start();
        OnLocationGenerationStarted?.Invoke(location);

        SetSeed();
        var generationData = new GenerationData(location, transition, _debugStairs);
        var presetName = location.GenerationPreset;
        var presets = GetComponentsInChildren<GenerationPreset>();

        foreach (var item in presets)
        {
            if (item.GenerationPresetName == presetName)
            {
                item.Generate(generationData);
                break;
            }
        }

        globalStopWatch.Stop();
        location.IsGenerated = true;
        print("Generation process completed and took " + globalStopWatch.ElapsedMilliseconds + " ms");

        OnLocationGenerationComplete?.Invoke(location);
    }

    public void GenerateFromEditor()
    {
        LocationMap.Clear();

        if (!Application.isPlaying)
        {
            DatabaseController.StartUpDatabases();
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];

        Generate(new Location(_testLocation.Id, _testLocation.Level, _testLocation.GenerationPreset, _testLocation.HyperstuctureName), null);
    }

    public void SetSeed()
    {
        if (_alwaysRandomSeed)
        {
            _seed = (int)DateTime.Now.Ticks;
            Random.InitState(_seed);
        }
        else
        {
            Random.InitState(_seed);
        }
    }
}

public class GenerationData
{
    public readonly Location Location;
    public readonly Transition Transition;
    public readonly bool DebugStairs;
    public List<TileData> Arcs = new List<TileData>();
    public List<Region> Regions = new List<Region>();
    public float[,] ElevationMap = new float[64, 64];
    public TileData StaircaseDebugTile;

    public GenerationData(Location location, Transition transition, bool debugStairs)
    {
        Location = location;
        Transition = transition;
    }
}

public static class LocationLoader
{
    public static event Action<Location> OnLocationLoaded;

    public static void LoadLocation(LocationSaveData locationSaveData)
    {
        LocationMap.Clear();

        foreach (var tile in locationSaveData.Tiles)
        {
            LocationMap.SetTileData(tile);
        }
        foreach (var data in locationSaveData.SimpleObjects)
        {
            if (data != null && data.SimpleObjectName != SimpleObjectName.Null)
            {
                Spawner.Spawn(data);
            }
        }

        ManualCommanBufferSytem.Instance.Update();

        OnLocationLoaded?.Invoke(locationSaveData.Location);
        TimeTester.EndTest("Loading location complete: ");
    }
}