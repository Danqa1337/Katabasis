using UnityEngine;

public class DatabaseController : Singleton<DatabaseController>
{
    [SerializeField] private ScriptableObject[] _databases;
    public static void StartUpDatabases()
    {
        foreach (var item in instance._databases)
        {
            (item as IStartUp).StartUp();
        }
    }
}