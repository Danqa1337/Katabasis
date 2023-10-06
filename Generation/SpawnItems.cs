using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;

namespace GenerationModules
{
    public class SpawnItems : GenerationModule
    {
        [SerializeField] private float _itemPer100FloorTiles = 0.6f;

        public override void Generate(GenerationData generationData)
        {
            if (Application.isPlaying)
            {
                var names = new List<string>();
                int e = 0;
                for (int i = 0; i < _itemPer100FloorTiles * GetFloorTilesNum() * 0.01f; i++)
                {
                    TileData tile = GetTile(t => t.SolidLayer == Entity.Null && t.FloorLayer != Entity.Null);
                    var itemData = SimpleObjectsDatabase.GetRandomSimpleObject(generationData.Location.Level, tile.biome, new List<Tag>());

                    Entity entity = Spawner.Spawn(itemData);
                    if (Application.isPlaying) names.Add(entity.GetName());
                    e = i;
                }
                var message = e + " items spawned: ";
                foreach (var item in names)
                {
                    message += ", " + item;
                }
                CheatConsoleScreen.print(message);
            }
        }
    }
}