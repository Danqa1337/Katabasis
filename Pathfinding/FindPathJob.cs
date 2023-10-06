using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FindPathJob : IJob
{
    [ReadOnly] public int tryLimit;
    [ReadOnly] public int2 startPosition;
    [ReadOnly] public int2 targetPosition;
    [ReadOnly] public WalkabilityDataComponent walkabilityData;
    [ReadOnly] public BlobAssetReference<TileBlobAsset> map;
    public PathFinderPath path;

    public void Execute()
    {
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

        int t = 0;

        while (openList.Length > 0 && t < tryLimit)
        {
            t++;
            int currentIndex = Pathfinder.GetLowestFCostIndex(openList, pathNodeArray, targetPosition);
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
                for (int i = 0; i < 8; i++)
                {
                    int2 offset = LocationMap.NeiborsOffsetsArray8[i];
                    int2 position = new int2(currentNode.X + offset.x, currentNode.Y + offset.y);

                    int neiborIndex = position.ToMapIndex();

                    //check valid position
                    if (neiborIndex == -1) continue;
                    if (closedList.Contains(neiborIndex)) continue;

                    PathNode neiborNode = pathNodeArray[neiborIndex];

                    if (!map.Value.blobArray[neiborNode.index].isWalkableBurstSafe(walkabilityData, currentNode.position)
                        && !(position.Equals(startPosition) || position.Equals(targetPosition))) continue;

                    var tentativeGCost = currentNode.gCost + Pathfinder.GetHCost(currentNode, neiborNode);
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

        PathNode endNode = pathNodeArray[endNodeIndex];

        if (endNode.parentIndex == -1)
        {
        }
        else
        {
            var retraced = RetracePath(pathNodeArray, endNode);
            path.CopyFrom(retraced);
            retraced.Dispose();
        }

        pathNodeArray.Dispose();
        openList.Dispose();
        closedList.Dispose();
    }

    private static NativeList<int2> RetracePath(NativeArray<PathNode> pathnodeArray, PathNode endNode)
    {
        if (endNode.parentIndex == -1)
        {
            //Path Not Found

            return new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            //Path Found
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            NativeList<int2> reversedPath = new NativeList<int2>(Allocator.Temp);

            reversedPath.Add(new int2(endNode.X, endNode.Y));
            PathNode currentNode = endNode;

            while (currentNode.parentIndex != -1)
            {
                PathNode parent = pathnodeArray[currentNode.parentIndex];
                reversedPath.Add(new int2(currentNode.X, currentNode.Y));
                currentNode = parent;
            }

            for (int i = reversedPath.Length - 1; i >= 0; i--)
            {
                path.Add(reversedPath[i]);
            }

            reversedPath.Dispose();
            return path;
        }
    }
}