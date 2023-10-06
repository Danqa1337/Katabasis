using Unity.Entities;
using System;

namespace GenerationModules
{
    public class ConnectRegionsWithPassages : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            while (generationData.Regions.Count > 1)
            {
                var currentRegion = generationData.Regions[0];
                var currentRegionsCenter = currentRegion.GetCenter();
                var minSqrDst = float.MaxValue;
                var nearestRegion = currentRegion;
                foreach (var region in generationData.Regions)
                {
                    if (region == currentRegion) continue;
                    var nearestTileSqrDst = float.MaxValue;
                    region.GetNearestTile(currentRegionsCenter, out nearestTileSqrDst);

                    if (nearestTileSqrDst < minSqrDst)
                    {
                        minSqrDst = nearestTileSqrDst;
                        nearestRegion = region;
                    }
                }

                var path = ConnectRegionsWithPath(currentRegion, nearestRegion);
                if (path != PathFinderPath.Null)
                {
                    ExpandPassage(path, 60);
                    TurnPathIntoCavePassage(path);
                    path.Dispose();
                }
                generationData.Regions.Remove(currentRegion);
                generationData.Regions.Remove(nearestRegion);
                generationData.Regions.Add(new Region(currentRegion, nearestRegion));
            }
            if (Debug) UnityEngine.Debug.Log(generationData.Regions.Count + " regions left");
        }

        public virtual PathFinderPath ConnectRegionsWithPath(Region region1, Region region2)
        {
            if (region1 == region2) throw new Exception("trying to connect the seme region");
            if (region1.Size == 0 || region2.Size == 0) throw new Exception("Region size is 0");

            var nearestTileToSecondRegionCenter = region1.GetNearestTile(region2.GetCenter());
            var nearestTileToFirstRegionNearestTile = region2.GetNearestTile(region1.GetCenter());

            if (nearestTileToFirstRegionNearestTile == nearestTileToSecondRegionCenter)
            {
                return PathFinderPath.Null;
            }

            PathFinderPath path = Pathfinder.FindPathNoJob(nearestTileToSecondRegionCenter.position, nearestTileToFirstRegionNearestTile.position, (t => t.SolidLayer == Entity.Null ? 1 : -1), tryLimit: 10000, debug: true);
            if (path == PathFinderPath.Null || !path.IsCreated || path.Nodes.Length == 0)
            {
                throw new NullReferenceException("Path connecting regions is null");
            }

            foreach (var item in path.Nodes)
            {
                region1.Add(item.ToTileData());
            }

            return path;
        }
    }
}