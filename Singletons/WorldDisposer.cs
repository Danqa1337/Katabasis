using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class WorldDisposer : Singleton<WorldDisposer>
{
    public static void DisposeWorld()
    {
        TimeTester.StartTest();

        if (Application.isPlaying)
        {
            foreach (var item in World.DefaultGameObjectInjectionWorld.EntityManager.GetAllEntities())
            {
                item.Destroy();
            };
        }
        TimeTester.EndTest("World disposing took ");
    }

    public void OnApplicationQuit()
    {
        DisposeWorld();
    }
}