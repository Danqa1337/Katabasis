using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using System;

namespace GenerationModules
{
    public class DefineRegions : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            var oldRegionsCount = generationData.Regions.Count;
            generationData.Regions = new List<Region>();
            var closedSet = new NativeList<TileData>(1024, Allocator.Temp);
            var openList = new NativeList<TileData>(1024, Allocator.Temp);
            var freeTiles = new NativeList<TileData>(1024, Allocator.Temp);
            var includeInRegionPredicate = new Func<TileData, bool>(t => t.Template.isFloor() && !t.Template.doNotIncludeInRegions);

            foreach (var VARIABLE in GetAllMapTiles().Where(includeInRegionPredicate))
            {
                freeTiles.Add(VARIABLE);
            }

            while (freeTiles.Length > 0)
            {
                closedSet.Clear();
                openList.Clear();
                var newRegion = new Region();
                var newTile = freeTiles[0];

                openList.Add(newTile);

                while (openList.Length > 0)
                {
                    newTile = openList[0];
                    freeTiles.Remove(newTile);
                    openList.Remove(newTile);
                    newRegion.Tiles.Add(newTile);

                    closedSet.Add(newTile);

                    foreach (var item in newTile.GetNeibors(true))
                    {
                        if (freeTiles.Contains(item)) openList.Add(item);
                    }
                }
                generationData.Regions.Add(newRegion);
            }

            freeTiles.Dispose();
            closedSet.Dispose();
            openList.Dispose();

            if (Debug)
            {
                var currentRegionsCount = generationData.Regions.Count;
                var newRegionsCount = currentRegionsCount - oldRegionsCount;

                UnityEngine.Debug.Log(currentRegionsCount + " Regions found. " + newRegionsCount + " regions are new");
            }
        }
    }
}