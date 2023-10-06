using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

[DisableAutoCreation]
public partial class SpawnSystem : MySystemBase
{
    private static List<(SimpleObjectData, int2)> _scheduledSimpleObjects = new List<(SimpleObjectData, int2)>();
    private static List<(ComplexObjectData, int2)> _scheduledComplexObjects = new List<(ComplexObjectData, int2)>();
    protected override void OnUpdate()
    {
        foreach (var item in _scheduledSimpleObjects)
        {
            Spawner.Spawn(item.Item1, item.Item2.ToTileData());
        }
        _scheduledSimpleObjects.Clear(); 

        foreach (var item in _scheduledComplexObjects)
        {
            Spawner.Spawn(item.Item1, item.Item2.ToTileData());
        }
        _scheduledComplexObjects.Clear();
    }
    public static void ScheduleSpawn(SimpleObjectData data, TileData tile)
    {
        _scheduledSimpleObjects.Add((data, tile.position));
    }
    public static void ScheduleSpawn(SimpleObjectName name, TileData tile)
    {
        ScheduleSpawn(SimpleObjectsDatabase.GetObjectData(name, true), tile);
    }
    public static void ScheduleSpawn(ComplexObjectData data, TileData tile)
    {
        _scheduledComplexObjects.Add((data, tile.position));
    }
    public static void ScheduleSpawn(ComplexObjectName name, TileData tile)
    {
        ScheduleSpawn(ComplexObjectsDatabase.GetObjectData(name, true), tile);
    }
}
