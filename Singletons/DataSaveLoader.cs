using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Application = UnityEngine.Application;

public class DataSaveLoader : Singleton<DataSaveLoader>
{
    private static string SavesFolderPath => Application.persistentDataPath + "/Saves/";
    private Task _currentSaveOperation = Task.CompletedTask;
    public static bool IsOperating => !instance._currentSaveOperation.IsCompleted;

    private void Start()
    {
        if (!Directory.Exists(SavesFolderPath))
        {
            Directory.CreateDirectory(SavesFolderPath);
        }
    }

    public static bool SaveExists<T>() where T : class
    {
        var path = SavesFolderPath + typeof(T);
        return File.Exists(path);
    }

    public static bool SaveExists<T>(string locationName) where T : class
    {
        var path = SavesFolderPath + typeof(T) + locationName;
        return File.Exists(path);
    }

    public static T LoadData<T>() where T : class
    {
        var path = SavesFolderPath + typeof(T);
        return BinarySerializer.Read<T>(path);
    }

    public static T LoadData<T>(string locationName) where T : class
    {
        var path = SavesFolderPath + typeof(T) + locationName;
        return BinarySerializer.Read<T>(path);
    }

    public static void SaveSaveData(object saveData)
    {
        var type = saveData.GetType();
        var path = SavesFolderPath + type;
        if (type == typeof(LocationSaveData))
        {
            path += (saveData as LocationSaveData).Location.Name;
        }
        BinarySerializer.Write(saveData, path);
    }

    public static void SaveCurrentLocation()
    {
        SaveSaveData(LocationSaveDataCreator.SaveCurrentLocation());
    }

    public static void DeleteSaves()
    {
        var names = Directory.GetFiles(SavesFolderPath);
        foreach (var name in names)
        {
            File.Delete(name);
        }
        Debug.Log("Saves deleted");
    }

    public static async void SaveAll()
    {
        if (!instance._currentSaveOperation.IsCompleted)
        {
            Debug.Log("awaiting existing saving operation");
            await instance._currentSaveOperation;
        }
        Debug.Log("Saving started");

        SaveSaveData(LocationSaveDataCreator.CreatePlayerSquadSaveData());

        instance._currentSaveOperation = Task.Factory.StartNew(() =>
        {
            Debug.Log("Saving location...");
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            SaveCurrentLocation();

            stopWatch.Stop();
            Debug.Log("Saving location completed, took " + stopWatch.ElapsedMilliseconds);
        });
    }
}

[System.Serializable]
public class PlayerSquadSaveData
{
    [SerializeField] public readonly ComplexObjectData Player;
    [SerializeField] public readonly List<ComplexObjectData> squadMates;
    public TileData PlayerTile => Player != null ? Player.Body.currentTileComponent.CurrentTile : TileData.Null;

    public PlayerSquadSaveData(ComplexObjectData player, List<ComplexObjectData> squadMates)
    {
        this.Player = player;
        this.squadMates = squadMates;
    }
}

[System.Serializable]
public class LocationSaveData
{
    public readonly Location Location;
    public readonly TileData[] Tiles;
    public readonly SimpleObjectData[] SimpleObjects;
    public readonly ComplexObjectData[] ComplexObjects;

    public LocationSaveData(Location location, TileData[] tiles, SimpleObjectData[] simpleObjects, ComplexObjectData[] complexObjects)
    {
        Location = location;
        Tiles = tiles;
        SimpleObjects = simpleObjects;
        ComplexObjects = complexObjects;
    }
}

[System.Serializable]
public class MiscStatsSaveData
{
    public int currentXp;
    public int currentLVL;
    public int freeStatPoints;
    public int CurrentTime;
    public int CurrentGnosis;
}