using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices.ComTypes;
using Unity.Entities;

public static class Spawner
{
    public static Action OnPlayersSquadSpawned;
    public static Action<Entity> OnPlayerSpawned;

    private static readonly SimpleObjectsSpawner _simpleObjectsSpawner = new SimpleObjectsSpawner();
    private static readonly ComplexObjectsSpawner _complexObjectsSpawner = new ComplexObjectsSpawner();
    private static readonly PacksSpawner _packsSpawner = new PacksSpawner();

    public static Entity Spawn(SimpleObjectName simpleObjectName, TileData tile)
    {
        var simpleObjectData = SimpleObjectsDatabase.GetObjectData(simpleObjectName);

        return _simpleObjectsSpawner.Spawn(simpleObjectData, tile);
    }

    public static Entity Spawn(SimpleObjectData simpleObjectData, TileData currentTile)
    {
        return _simpleObjectsSpawner.Spawn(simpleObjectData, currentTile);
    }

    public static Entity Spawn(ComplexObjectName complexObjectName, TileData tile)
    {
        var complexObjectData = ComplexObjectsDatabase.GetObjectData(complexObjectName);
        return _complexObjectsSpawner.Spawn(complexObjectData, tile);
    }

    public static Entity Spawn(SimpleObjectData simpleObjectData)
    {
        return _simpleObjectsSpawner.Spawn(simpleObjectData, simpleObjectData.currentTileComponent.CurrentTile);
    }

    public static Entity Spawn(ComplexObjectData complexObjectData, TileData currentTile)
    {
        return _complexObjectsSpawner.Spawn(complexObjectData, currentTile);
    }

    public static Entity Spawn(ComplexObjectData complexObjectData)
    {
        return _complexObjectsSpawner.Spawn(complexObjectData, complexObjectData.Body.currentTileComponent.CurrentTile);
    }

    public static List<Entity> SpawnPack(PackName packName, TileData center)
    {
        return _packsSpawner.SpawnPack(ComplexObjectsPacksDatabase.GetPackData(packName), center);
    }

    public static List<Entity> SpawnPack(PackData<ComplexObjectData> packData, TileData center)
    {
        return _packsSpawner.SpawnPack(packData, center);
    }

    public static void SpawnPlayerSquad(PlayerSquadSaveData playerSquadSaveData, TileData tileData)
    {
        var player = _packsSpawner.SpawnPlayerSquad(playerSquadSaveData, tileData);
        OnPlayerSpawned?.Invoke(player);
        OnPlayersSquadSpawned?.Invoke();
    }
}

public class SpawnerUtility
{
    protected void AddComponent<Tcomponent>(Entity entity, ComponentReferece<Tcomponent> componentReferece) where Tcomponent : unmanaged, IComponentData
    {
        if (componentReferece.IsValid)
        {
            entity.AddComponentData(componentReferece.Component);
        }
    }

    protected void AddBuffer<TElement>(Entity entity, List<TElement> elements) where TElement : unmanaged, IBufferElementData
    {
        entity.AddBuffer<TElement>();

        foreach (var item in elements)
        {
            entity.AddBufferElement(item);
        }
    }
}