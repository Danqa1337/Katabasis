using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.Entities;

namespace GenerationModules
{
    public class SpawnCreatures : GenerationModule
    {
        [SerializeField] private float _creaturePer100FloorTiles = 1f;

        public override void Generate(GenerationData generationData)
        {
            if (Application.isPlaying)
            {
                int creaturesSpawned = 0;
                int packsSpawned = 0;
                var creauresToSpawn = _creaturePer100FloorTiles * GetFloorTilesNum() * 0.01f;
                var names = new List<string>();

                for (int i = 0; i < creauresToSpawn; i++)
                {
                    TileData tile = GetTile(t => t.SolidLayer == Entity.Null && !t.isAbyss);

                    var pack = ComplexObjectsPacksDatabase.GetRandomPackData(generationData.Location.Level, tile.biome);
                    Spawner.SpawnPack(pack, tile);

                    i += pack.Count;
                    creaturesSpawned += pack.Count;
                    packsSpawned++;
                    names.AddRange(pack.members.Select(m => m.ComplexObjectName.ToString()));
                }

                var message = creaturesSpawned + " creatures spawned in " + packsSpawned + " packs : ";
                foreach (var item in names)
                {
                    message += ", " + item;
                }
                CheatConsoleScreen.print(message);
            }
        }
    }
}