using System.Collections;
using UnityEngine;

public class SceneStarter : Singleton<SceneStarter>
{
    [SerializeField] private LaunchMode _launchMode;

    private void Awake()
    {
        DatabaseController.StartUpDatabases();

        if (DataSaveLoader.SaveExists<RegistersData>())
        {
            Registers.Load();
        }
        else
        {
            Registers.SetDefaultValues();
        }
    }

    private async void Start()
    {
        Pooler.UpdatePools();

        if (UndestructableData.instance != null)
        {
            _launchMode = UndestructableData.instance.launchMode;
        }

        if (_launchMode == LaunchMode.Dungeon)
        {
            LocationEnterManager.MoveToLocation(Registers.GlobalMapRegister.CurrentLocation, null);
        }
        else
        {
            LocationEnterManager.MoveToLocation(Registers.GlobalMapRegister.Arena, null, true);
        }
    }

    public static void SetLaunchMode(LaunchMode launchMode)
    {
        instance._launchMode = launchMode;
    }
}