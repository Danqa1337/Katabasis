using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public static class Pathfinder
{
    public static PathFinderPath FindPath(TileData startTile, TileData targetTile, WalkabilityDataComponent walkabilityData, int tryLimit = 100, Allocator allocator = Allocator.TempJob)
    {
        var path = new PathFinderPath(allocator);
        var map = LocationMap.MapRefference;
        var jobHandle = SchedulePathFinding(path, map, walkabilityData, startTile, targetTile, tryLimit);
        jobHandle.Complete();
        return path;
    }

    public static PathFinderPath FindPathNoJob(int2 startPosition, int2 targetPosition, Func<TileData, float> walkabilityPredicate, int tryLimit = 100, bool canMoveDiagonaly = false, Allocator allocator = Allocator.Persistent, bool debug = false)
    {
        if (startPosition.Equals(targetPosition))
        {
            throw new Exception("Trying to find path, but start and target are equal");
        }
        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(4096, Allocator.Temp);
        for (int i = 0; i < 4096; i++)
        {
            pathNodeArray[i] = new PathNode(i, startPosition, targetPosition);
        }

        int endNodeIndex = targetPosition.ToMapIndex();
        PathNode startNode = pathNodeArray[startPosition.ToMapIndex()];
        startNode.gCost = 0;
        startNode.CalculateFCost();
        pathNodeArray[startNode.index] = startNode;

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        openList.Add(startNode.index);
        int offestsCount = canMoveDiagonaly ? 8 : 4;
        var offsets = canMoveDiagonaly ? LocationMap.NeiborsOffsetsArray8 : LocationMap.NeiborsOffsetsArray4;

        int t = 0;

        while (openList.Length > 0 && t < tryLimit)
        {
            t++;
            int currentIndex = GetLowestFCostIndex(openList, pathNodeArray, targetPosition);
            PathNode currentNode = pathNodeArray[currentIndex];

            if (currentIndex == endNodeIndex)
            {
                //Path Completed!
                break;
            }
            else
            {
                //Remove current from openlist
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentIndex);

                for (int i = 0; i < offestsCount; i++)
                {
                    int2 offset = offsets[i];
                    int2 position = new int2(currentNode.X + offset.x, currentNode.Y + offset.y);
                    int neiborIndex = position.ToMapIndex();

                    //check valid position
                    if (neiborIndex == -1) continue;
                    if (closedList.Contains(neiborIndex)) continue;

                    PathNode neiborNode = pathNodeArray[neiborIndex];

                    var neiborTile = neiborNode.index.ToTileData();
                    if (walkabilityPredicate(neiborTile) < 0
                        && !(position.Equals(startPosition) || position.Equals(targetPosition))) continue;

                    var tentativeGCost = currentNode.gCost + GetHCost(currentNode, neiborNode) * walkabilityPredicate(neiborTile);
                    if (tentativeGCost < neiborNode.gCost)
                    {
                        neiborNode.parentIndex = currentNode.index;
                        neiborNode.gCost = tentativeGCost;
                        neiborNode.CalculateFCost();
                        pathNodeArray[neiborNode.index] = neiborNode;

                        if (!openList.Contains(neiborNode.index))
                        {
                            openList.Add(neiborNode.index);
                        }
                    }
                }
            }
        }
        if (debug && t >= tryLimit)
        {
            UnityEngine.Debug.Log("out of try limit");
        }
        PathNode endNode = pathNodeArray[endNodeIndex];

        var path = new PathFinderPath(allocator);
        path.CopyFrom(RetracePath(pathNodeArray, endNode));
        pathNodeArray.Dispose();
        openList.Dispose();
        closedList.Dispose();
        return path;

        NativeList<int2> RetracePath(NativeArray<PathNode> pathnodeArray, PathNode endNode)
        {
            if (endNode.parentIndex == -1)
            {
                //Path Not Found
                return new NativeList<int2>(Allocator.Temp);
            }
            else
            {
                //Path Found
                NativeList<int2> path = new NativeList<int2>(Allocator.Persistent);
                NativeList<int2> reversedPath = new NativeList<int2>(Allocator.Temp);

                reversedPath.Add(new int2(endNode.X, endNode.Y));
                PathNode currentNode = endNode;

                while (currentNode.parentIndex != -1)
                {
                    PathNode parent = pathnodeArray[currentNode.parentIndex];
                    reversedPath.Add(new int2(currentNode.X, currentNode.Y));
                    currentNode = parent;
                }
                reversedPath.Add(new int2(currentNode.X, currentNode.Y));
                for (int i = reversedPath.Length - 1; i >= 0; i--)
                {
                    path.Add(reversedPath[i]);
                }

                reversedPath.Dispose();
                return path;
            }
        }
    }

    public static float GetHCost(PathNode A, PathNode B)
    {
        return GetHCost(A.X, A.Y, B.X, B.Y);
    }

    public static float GetHCost(int2 A, int2 B)
    {
        return GetHCost(A.x, A.y, B.x, B.y);
    }

    public static float GetHCost(int AX, int AY, int BX, int BY)
    {
        var dstX = math.abs(AX - BX);
        var dstY = math.abs(AY - BY);
        var remaining = math.abs(dstX - dstY);
        var D = 100;
        var D2 = 101;
        var heuristic = D * (dstX + dstY) + (D2 - 2 * D) * math.min(dstX, dstY);

        return heuristic;
    }

    public static int GetLowestFCostIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray, int2 targetPosition)
    {
        PathNode lowestFNode = pathNodeArray[openList[0]];
        for (int i = 0; i < openList.Length; i++)
        {
            PathNode testNode = pathNodeArray[openList[i]];
            if (testNode.fCost < lowestFNode.fCost)
            {
                lowestFNode = testNode;
            }
        }
        return lowestFNode.index;
    }

    public static JobHandle SchedulePathFinding(PathFinderPath path, BlobAssetReference<TileBlobAsset> map, WalkabilityDataComponent walkabilityData, TileData startNode, TileData targetNode, int tryLimit)
    {
        return SchedulePathFinding(path, map, walkabilityData, startNode.position, targetNode.position, tryLimit);
    }

    public static JobHandle SchedulePathFinding(PathFinderPath path, BlobAssetReference<TileBlobAsset> map, WalkabilityDataComponent walkabilityData, int2 startNode, int2 targetNode, int tryLimit)
    {
        if (startNode.Equals(targetNode))
        {
            throw new Exception("Trying to find path, but start and target are equal");
        }

        var job = new FindPathJob()
        {
            tryLimit = tryLimit,
            startPosition = startNode,
            targetPosition = targetNode,
            path = path,
            map = map,
            walkabilityData = walkabilityData,
        };

        return job.Schedule();
    }
}