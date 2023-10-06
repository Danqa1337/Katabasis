using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Region
{
    public HashSet<TileData> Tiles { get; private set; }
    public int Size { get => Tiles.Count; }

    public Region(HashSet<TileData> _tiles)
    {
        Tiles = _tiles;
    }

    public Region()
    {
        Tiles = new HashSet<TileData>();
    }

    public Region(Region r1, Region r2)
    {
        Tiles = new HashSet<TileData>();
        foreach (var item in r1.Tiles.Concat(r2.Tiles))
        {
            Tiles.Add(item);
        }
    }

    public void Add(IEnumerable<TileData> tiles)
    {
        foreach (var VARIABLE in tiles)
        {
            if (!tiles.Contains(VARIABLE))
            {
                this.Tiles.Add(VARIABLE);
            }
        }
    }

    public void Add(TileData tile)
    {
        Tiles.Add(tile);
    }

    public TileData GetCenter()
    {
        if (Tiles.Count == 0)
        {
            throw new System.Exception("Region size is 0");
        }

        List<int2> positions = Tiles.Select(t => t.position).ToList();
        int2 sum = int2.zero;
        foreach (var item in positions)
        {
            sum += item;
        }
        return (sum / positions.Count).ToTileData();
    }
    public TileData GetNearestTile(TileData target)
    {
        var sqrDst = 0f;
        return GetNearestTile(target, out sqrDst);
    }
    public TileData GetNearestTile(TileData target, out float minSqrDst)
    {
        if(!target.valid)
        {
            throw new System.Exception("Target tile is invalid");
        }
        if(Tiles.Count == 0)
        {
            throw new System.Exception("Region size is 0");
        }

        var result = Tiles.First();
        minSqrDst = float.MaxValue;
        foreach (var tile in Tiles)
        {
            var sqrDst = tile.GetSqrDistance(target);
            if(sqrDst < minSqrDst)
            {
                minSqrDst = sqrDst;
                result = tile;
            }
        }
        return result;
    }
}