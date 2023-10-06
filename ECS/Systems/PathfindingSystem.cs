using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisableAutoCreation]
public partial class PathfindingSystem : MySystemBase
{
    private ManualCommanBufferSytem _manualCommanBufferSytem;

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnUpdate()
    {
        _debug = LowLevelSettings.instance.debugPathfinding;

        var query = GetEntityQuery(
            ComponentType.ReadWrite<FindPathTag>(),
            ComponentType.ReadWrite<WalkabilityDataComponent>(),
            ComponentType.ReadWrite<AIComponent>(),
            ComponentType.ReadWrite<PathComponent>()

            );

        new FindPathParallelJobEntity()
        {
            map = LocationMap.MapRefference,
        }.ScheduleParallel(query);

        Dependency.Complete();

        var ecb = _manualCommanBufferSytem.CreateCommandBuffer().AsParallelWriter();
        Entities.WithAll<FindPathTag>().ForEach((int entityInQueryIndex, Entity entity) =>
        {
            ecb.RemoveComponent<FindPathTag>(entityInQueryIndex, entity);
        }).WithoutBurst().ScheduleParallel();
        Dependency.Complete();

        if (_debug && !query.IsEmpty)
        {
            var entities = query.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var findPathComponent = entities[i].GetComponentData<PathComponent>();
                if (_debug) NewDebugMessage(entities[i].GetName() + " searched path from " + findPathComponent.start + " to " + findPathComponent.target + ". Result: " + findPathComponent.found);
            }

            entities.Dispose();
        }
        _manualCommanBufferSytem.Update();
        WriteDebug();
    }
}

public struct PathComponent : IComponentData
{
    public AiBehaviourName behvaiour;
    public int2 start;
    public int2 target;
    public int2 pathPosition_0;
    public int2 pathPosition_1;
    public int2 pathPosition_2;
    public int pathIndex;

    public bool found;

    public int2 CurrentPathPosition
    {
        get
        {
            if (pathIndex == 0) return pathPosition_0;
            if (pathIndex == 1) return pathPosition_1;
            if (pathIndex == 2) return pathPosition_2;
            return new int2(-1, -1);
        }
    }

    public PathComponent(int2 start, int2 traget)
    {
        this.behvaiour = AiBehaviourName.Wait;
        this.start = start;
        this.target = traget;
        this.pathPosition_0 = int2.zero;
        this.pathPosition_1 = int2.zero;
        this.pathPosition_2 = int2.zero;
        this.pathIndex = 0;
        this.found = false;
    }

    public PathComponent(bool complite)
    {
        this.behvaiour = AiBehaviourName.Wait;
        this.start = int2.zero;
        this.target = int2.zero;
        this.pathPosition_0 = int2.zero;
        this.pathPosition_1 = int2.zero;
        this.pathPosition_2 = int2.zero;
        this.pathIndex = 0;
        this.found = complite;
    }
}

[BurstCompile]
public partial struct FindPathParallelJobEntity : IJobEntity
{
    [ReadOnly] public BlobAssetReference<TileBlobAsset> map;

    public void Execute(ref PathComponent findPathComponent, ref AIComponent aIComponent, in WalkabilityDataComponent walkabilityData)
    {
        if (findPathComponent.found) return;
        var startPosition = findPathComponent.start;
        var targetPosition = findPathComponent.target;

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

        while (openList.Length > 0 && t < aIComponent.tryLimit)
        {
            t++;
            int currentIndex = Pathfinder.GetLowestFCostIndex(openList, pathNodeArray, targetPosition);
            var currentNode = pathNodeArray[currentIndex];

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

                    var neiborNode = pathNodeArray[neiborIndex];

                    if (!map.Value.blobArray[neiborNode.index].isWalkableBurstSafe(walkabilityData, startPosition)
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
            findPathComponent.found = false;
            aIComponent.tryLimit = Mathf.Min(aIComponent.tryLimit * 2, AIComponent.maxTryLimit);
        }
        else
        {
            //Save 4 last path nodes which we can reuse
            NativeList<int2> reversedPath = new NativeList<int2>(Allocator.Temp);

            reversedPath.Add(new int2(endNode.X, endNode.Y));
            PathNode currentNode = endNode;
            PathNode lastNode_0 = endNode;
            PathNode lastNode_1 = endNode;
            PathNode lastNode_2 = endNode;

            while (currentNode.parentIndex != -1)
            {
                PathNode parent = pathNodeArray[currentNode.parentIndex];
                lastNode_2 = lastNode_1;
                lastNode_1 = lastNode_0;
                lastNode_0 = currentNode;
                currentNode = parent;
            }

            findPathComponent.found = true;
            aIComponent.tryLimit = AIComponent.minTrylimit;
            findPathComponent.pathPosition_0 = lastNode_0.position;
            findPathComponent.pathPosition_1 = lastNode_1.position;
            findPathComponent.pathPosition_2 = lastNode_2.position;
        }

        pathNodeArray.Dispose();
        openList.Dispose();
        closedList.Dispose();
    }
}