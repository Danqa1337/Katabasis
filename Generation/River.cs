using UnityEngine;
using Unity.Entities;
using System;
using Unity.Mathematics;

namespace GenerationModules
{
    public class River : GenerationModule
    {
        [SerializeField] private float _riverChance;
        [SerializeField] private int _riverWidth = 2;

        public override void Generate(GenerationData generationData)
        {
            if (Chance(_riverChance))
            {
                var direction1 = BaseMethodsClass.GetRandomDir(false);
                var direction2 = BaseMethodsClass.GetOpositeDirection(direction1);

                var start = BaseMethodsClass.GetMapBorderFromDirection(direction1).RandomItem();
                var end = BaseMethodsClass.GetMapBorderFromDirection(direction2).RandomItem();

                var path = Pathfinder.FindPathNoJob(start.position, end.position, new Func<TileData, float>(t => t.SolidLayer == Entity.Null ? 1 : -1), 10000);

                path.Add(end.position);
                for (int i = 0; i < path.Nodes.Length; i++)
                {
                    var newNode = path.Nodes[i] + (UnityEngine.Random.insideUnitCircle).ToInt2();
                    if (newNode.ToTileData().valid)
                    {
                        path.Nodes[i] = newNode;
                    }
                }
                for (int i = 0; i < _riverWidth; i++)
                {
                    var chance = i == _riverWidth - 1 ? 70 : 100;
                    ExpandPassage(path, chance);
                }

                foreach (var item in path.Nodes)
                {
                    item.ToTileData().Template.GenerateObject(SimpleObjectName.BlackSandFloor);
                }

                foreach (var tile in path.Nodes)
                {
                    tile.ToTileData().Template.GenerateObject(SimpleObjectName.ShallowWater);

                    tile.ToTileData().Template.SetBiome(Biome.FloodedCave);
                }
                path.Dispose();
            }
        }
    }
}