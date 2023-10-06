using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class PacksSpawner : SpawnerUtility
{
    public Entity SpawnPlayerSquad(PlayerSquadSaveData playerSquadSaveData, TileData tile)
    {
        var position = tile.position;
        var playerEntity = Spawner.Spawn(playerSquadSaveData.Player, position.ToTileData());
        var squadSize = 1;

        if (Application.isPlaying)
        {
            var tileData = playerSquadSaveData.Player.Body.currentTileComponent.CurrentTile;
            var walkData = playerEntity.GetComponentData<WalkabilityDataComponent>();
            playerEntity.RemoveComponent<AIComponent>();
            playerEntity.AddComponentData(new PlayerTag());
            walkData.isPlayer = true;
            playerEntity.SetComponentData(walkData);
            playerEntity.GetBuffer<TagBufferElement>().Add(new TagBufferElement(Tag.Player));

            var creatureComponent = playerEntity.GetComponentData<CreatureComponent>();

            if (playerSquadSaveData.squadMates.Count > 0)
            {
                var tilesAround = BaseMethodsClass.GetTilesInRadius(tileData, 3).Where(t => t.ClearLineOfSight(tileData) && t.SolidLayer == Entity.Null && !t.isAbyss).ToList();

                foreach (var squadMate in playerSquadSaveData.squadMates)
                {
                    if (tilesAround.Count == 0)
                    {
                        throw new System.NullReferenceException("there is not enough space to spawn players squad");
                    }

                    var tileAround = tilesAround.RandomItem();
                    tilesAround.Remove(tileAround);
                    tileAround.Spawn(squadMate);
                    squadSize++;
                }
            }
        }
        Debug.Log("players squad of " + squadSize + " creatures spawned on " + position);
        return playerEntity;
    }

    public List<Entity> SpawnPack(PackData<ComplexObjectData> packData, TileData center)
    {
        if (packData.Count == 0) throw new System.NullReferenceException("pack count is 0");

        var freeTilesInRadius = BaseMethodsClass.GetTilesInRadius(center, 3).Where(t => t.SolidLayer == Entity.Null && t.ClearLineOfSight(center) && !t.isAbyss).ToList();
        var entities = new List<Entity>();
        foreach (var data in packData.members)
        {
            var dataClone = BinarySerializer.MakeDeepClone(data);

            var tile = freeTilesInRadius.RandomItem();
            freeTilesInRadius.Remove(tile);
            entities.Add(Spawner.Spawn(dataClone, tile));
            if (freeTilesInRadius.Count == 0) break;
        }

        if (Application.isPlaying)
        {
            var index = Registers.SquadsRegister.GetSquadIndex(entities[0]);
            foreach (var item in entities)
            {
                Registers.SquadsRegister.MoveToSquad(index, item);
            }
        }

        return entities;
    }
}
