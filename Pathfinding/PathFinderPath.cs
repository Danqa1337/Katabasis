using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Mathematics;

[BurstCompile]
public struct PathFinderPath : IDisposable
{
    public bool IsCreated => _isCreated;
    private bool _isCreated;
    public static PathFinderPath Null;

    private NativeList<int2> _nodes;

    public PathFinderPath(IEnumerable<TileData> nodes, Allocator allocator = Allocator.Persistent)
    {
        _isCreated = true;
        _nodes = new NativeList<int2>(allocator);
        _nodes.CopyFromNBC(nodes.Select(t => t.position).ToArray());
    }

    public PathFinderPath(Allocator allocator = Allocator.Persistent)
    {
        _isCreated = true;
        _nodes = new NativeList<int2>(allocator);
    }

    public int2[] Nodes
    {
        get
        {
            var result = new int2[_nodes.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _nodes[i];
            }
            return result;
        }
    }

    public int Length
    {
        get
        {
            if (IsCreated) return _nodes.Length;
            else return 0;
        }
    }

    public int2 EndPosition
    {
        get
        {
            if (IsCreated) return _nodes[Nodes.Length - 1];
            throw new Exception("Path is not created");
        }
    }

    public int2 StartPosition
    {
        get
        {
            if (IsCreated) return _nodes[0];
            throw new Exception("Path is not created");
        }
    }

    public void Add(int2 node)
    {
        _nodes.Add(node);
    }

    public HashSet<TileData> waySide
    {
        get
        {
            HashSet<TileData> side = new HashSet<TileData>();
            foreach (var tile in _nodes)
            {
                foreach (var neibor in tile.ToTileData().GetNeibors(true))
                {
                    if (neibor.valid && !Nodes.Contains(neibor.position))
                    {
                        side.Add(neibor);
                    }
                }
            }

            return side;
        }
    }

    public static bool operator !=(PathFinderPath path1, PathFinderPath path2)
    {
        return !path1.Equals(path2);
    }

    public static bool operator ==(PathFinderPath path1, PathFinderPath path2)
    {
        return path1.Equals(path2) || (path1.Length == 0 && path2 == Null);
    }

    public void CopyFrom(NativeList<int2> input)
    {
        _nodes.CopyFrom(input);
    }

    public void Dispose()
    {
        if (IsCreated) _nodes.Dispose();
        _isCreated = false;
    }

    public override string ToString()
    {
        if (this == Null)
        {
            return "Path Null";
        }
        return String.Format("Path from {0} to {1}. {2} tiles long", StartPosition, EndPosition, Length);
    }
}