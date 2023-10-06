using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Locations;
using System;

public class LocationEnterManager
{
    public static event Action<Location> OnMovingToLocationStarted;

    public static event Action<Location> OnExitPrevLocationCompleted;

    public static event Action<Location> OnMovingToLocationCompleted;

    public static async void MoveToLocation(Location location, Transition transition, bool spawnPlayerOnRandomPosition = false)
    {
        Debug.Log("Moving to " + location);

        if (transition != null && !transition.HasExposedEnd)
        {
            throw new System.Exception(transition + " is not connected with " + location);
        }

        ExitCurrentLocation();

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        LocationMap.Clear();

        if (location.IsGenerated)
        {
            LocationLoader.LoadLocation(DataSaveLoader.LoadData<LocationSaveData>(location.Name));
            Registers.Load();

            var playerSquadSaveData = DataSaveLoader.LoadData<PlayerSquadSaveData>();
            var playerSpawnTile = playerSquadSaveData.PlayerTile;

            if (transition != null)
            {
                playerSpawnTile = transition.ExposedEndTile;
            }
            else
            if (spawnPlayerOnRandomPosition)
            {
                playerSpawnTile = BaseMethodsClass.GetFreeTile();
            }

            Spawner.SpawnPlayerSquad(playerSquadSaveData, playerSpawnTile);
            StairsInfo.UpdateInfo();
        }
        else
        {
            LocationGenerator.instance.Generate(location, transition);
            StairsInfo.Clear();
        }

        ManualSystemUpdater.Update<SeamlessTextureSystem>();

        FOVSystem.ScheduleFOWUpdate();

        await TimeController.UpdateTicks();
        DataSaveLoader.SaveAll();

        OnMovingToLocationCompleted?.Invoke(location);
        TimeController.SpendTime(1);
        Debug.Log("Entring location took " + stopwatch.ElapsedMilliseconds);
    }

    private static void ExitCurrentLocation()
    {
        if (Registers.GlobalMapRegister.CurrentLocation.IsGenerated)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            DataSaveLoader.SaveAll();

            OnExitPrevLocationCompleted?.Invoke(Registers.GlobalMapRegister.CurrentLocation);

            stopwatch.Stop();

            Debug.Log("Exiting location took " + stopwatch.ElapsedMilliseconds);
        }
    }

    public static void EnterTransition(Transition transition)
    {
        Debug.Log("Entering " + transition);
        MoveToLocation(transition.UnExposedEndLocation, transition);
    }
}