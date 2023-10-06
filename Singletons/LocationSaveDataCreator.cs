using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System.Collections.Generic;

public static class LocationSaveDataCreator
{
    private static World _saveWorld;
    private static EntityManager _saveEntityManager;
    private static NativeArray<TileData> _saveTilesArray;
    private static NativeArray<Entity> _complexObjectEntitiesArray;
    private static NativeArray<Entity> _simpleObjectEntitiesArray;

    private static void CreateSaveWorld()
    {
        DisposeSaveWorld();
        _saveWorld = new World("saveWorld");
        _saveEntityManager = _saveWorld.EntityManager;
        _saveEntityManager.CopyAndReplaceEntitiesFrom(World.DefaultGameObjectInjectionWorld.EntityManager);

        var simpleObjectQueryDesc = new EntityQueryDesc()
        {
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<SimpleObjectNameComponent>(),
                ComponentType.ReadOnly<CurrentTileComponent>(),
            },
            None = new ComponentType[]
            {
                ComponentType.ReadOnly<Parent>(),
                ComponentType.ReadOnly<InventoryComponent>(),
                ComponentType.ReadOnly<CreatureComponent>(),
                ComponentType.ReadOnly<AnatomyComponent>(),
            }
        };
        var complexObjectQueryDesc = new EntityQueryDesc()
        {
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<ComplexObjectNameComponent>(),
                ComponentType.ReadOnly<CurrentTileComponent>(),
            },
            Any = new ComponentType[]
            {
                ComponentType.ReadOnly<InventoryComponent>(),
                ComponentType.ReadOnly<CreatureComponent>(),
                ComponentType.ReadOnly<AnatomyComponent>(),
            },
            None = new ComponentType[]
            {
                ComponentType.ReadOnly<Parent>()
            }
        };

        _simpleObjectEntitiesArray = _saveEntityManager.CreateEntityQuery(simpleObjectQueryDesc).ToEntityArray(Allocator.TempJob);
        _complexObjectEntitiesArray = _saveEntityManager.CreateEntityQuery(complexObjectQueryDesc).ToEntityArray(Allocator.TempJob);

        _saveTilesArray = new NativeArray<TileData>(4096, Allocator.TempJob);
        _saveTilesArray.CopyFrom(LocationMap.MapRefference.Value.blobArray.ToArray());
    }

    private static void DisposeSaveWorld()
    {
        if (_simpleObjectEntitiesArray.IsCreated) _simpleObjectEntitiesArray.Dispose();
        if (_complexObjectEntitiesArray.IsCreated) _complexObjectEntitiesArray.Dispose();
        if (_saveWorld != null && _saveWorld.IsCreated)
        {
            _saveWorld.Dispose();
        }
        if (_saveTilesArray.IsCreated) _saveTilesArray.Dispose();
    }

    public static PlayerSquadSaveData CreatePlayerSquadSaveData()
    {
        var squad = Registers.SquadsRegister.GetPlayersSquad();

        var squadMates = new List<ComplexObjectData>();
        var player = EntitySerializer.SerializeAsComplexObject(Player.PlayerEntity, World.DefaultGameObjectInjectionWorld.EntityManager);

        foreach (var item in squad.members)
        {
            if (!item.IsPlayer())
            {
                squadMates.Add(EntitySerializer.SerializeAsComplexObject(item, World.DefaultGameObjectInjectionWorld.EntityManager));
            }
        }

        return new PlayerSquadSaveData(player, squadMates);
    }

    public static LocationSaveData SaveCurrentLocation()
    {
        CreateSaveWorld();
        Location location = Registers.GlobalMapRegister.CurrentLocation;

        var simpleObjects = new SimpleObjectData[_simpleObjectEntitiesArray.Length];
        var complexObjects = new ComplexObjectData[_complexObjectEntitiesArray.Length];

        var squadsSaveData = Registers.SquadsRegister;
        var tiles = LocationMap.MapRefference.Value.blobArray.ToArray();

        for (int i = 0; i < _simpleObjectEntitiesArray.Length; i++)
        {
            var entity = _simpleObjectEntitiesArray[i];
            simpleObjects[i] = EntitySerializer.SerializeAsSimpleObject(entity, _saveEntityManager);
            if (!entity.HasComponent<SquadMemberComponent>() || (entity.HasComponent<SquadMemberComponent>() && entity.GetComponentData<SquadMemberComponent>().squadIndex != 1))
            {
                simpleObjects[i] = EntitySerializer.SerializeAsSimpleObject(entity, _saveEntityManager);
            }
            else
            {
                simpleObjects[i] = null;
            }
        }
        for (int i = 0; i < _complexObjectEntitiesArray.Length; i++)
        {
            var entity = _complexObjectEntitiesArray[i];
            if (!entity.HasComponent<SquadMemberComponent>() || (entity.HasComponent<SquadMemberComponent>() && entity.GetComponentData<SquadMemberComponent>().squadIndex != 1))
            {
                complexObjects[i] = EntitySerializer.SerializeAsComplexObject(entity, _saveEntityManager);
            }
            else
            {
                complexObjects[i] = null;
            }
        }
        DisposeSaveWorld();
        return new LocationSaveData(location, tiles, simpleObjects, complexObjects);
    }
}