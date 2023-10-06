using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace GenerationModules
{
    public class Erosion : GenerationModule
    {
        [SerializeField] private SimpleObjectName _material;
        [SerializeField] private float _erosionChancePerFreeNeibor = 5;
        public override void Generate(GenerationData generationData)
        {
            var tilesToErode = new List<TileData>();
            foreach (var tile in GetAllMapTiles())
            {
                if (tile.Template.Biome == Biome.Wall)
                {
                    float chance = 0;
                    foreach (var item in tile.GetNeibors(true).Where(t => t != TileData.Null))
                    {
                        if (item.Template.isFloor())
                        {
                            chance += _erosionChancePerFreeNeibor;
                        }
                    }
                    if (Chance(chance))
                    {
                        tilesToErode.Add(tile);
                    }
                }
            }
            foreach (var tile in tilesToErode)
            {
                tile.Template.GenerateObject(_material);

            }
        }
    }

}

