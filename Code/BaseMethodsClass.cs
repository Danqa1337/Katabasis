using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum SortingLayer
{
    Default,
    Underground,
    Floor,
    GroundCover,
    Drop,
    Liquid,
    Solid,
    Hovering,
}

public enum BodyPartTag
{
    Null,
    Any,
    Head,
    RightArm,
    LeftArm,
    RightFrontLeg,
    RightRearLeg,
    LeftFrontLeg,
    LeftRearLeg,
    RightFrontPaw,
    RightRearPaw,
    LeftFrontPaw,
    LeftRearPaw,
    Body,
    Tail,
    Tentacle,
    FirstTentacle,
    SecondTentacle,
    RightClaw,
    LeftClaw,
    Teeth,
    Fists,
    LowerBody,
}

public enum Direction
{
    U,
    D,

    R,
    L,

    UL,
    UR,

    DR,
    DL,

    Null
}

public enum Scaling
{
    A,
    B,
    C,
    D,
    _,
}

public enum EquipTag
{
    Weapon,
    Shield,
    Headwear,
    Chestplate,
    None,
}

public enum ReadinessLevel
{
    Sleeping,
    Loitering,
    OnAllert,
    FullyConcentrated,
}

public enum Stance
{
    Free,
    Closed,
}

public enum HitType
{
    Normal,
    Crit,
}

public enum WeaponGrip
{
    High,
    Medium,
    Low,
}

public enum Gender
{
    Male,
    Female,
    It,
}

public enum TemplateState
{
    Null,
    Any,
    Floor,
    Wall,
    Door,
    ShallowWater,
    Darkness,
    Abyss,
    Passage,
}

public enum Size : int
{
    Tiny = 1,
    Small = 2,
    Medium = 3,
    Large = 4,
    Huge = 5,
}

public enum ObjectType
{
    Solid,
    Drop,
    Floor,
    Liquid,
    GroundCover,
    Hovering,
}

public static class BaseMethodsClass
{
    public static Quaternion GetRandomRotation(float r1 = 0, float r2 = 360)
    {
        return Quaternion.Euler(new Vector3(0, 0, UnityEngine.Random.Range(r1, r2)));
    }

    public static bool Chance(float percent)
    {
        if (percent == 0) return false;
        if (percent == 100) return true;
        if (percent >= UnityEngine.Random.Range(0, 10000) * 0.01) return true;
        return false;
    }

    public static Direction GetRandomDir(bool IncludeDiagonal, bool includeNull = false)
    {
        if (includeNull)
        {
            return (Direction)UnityEngine.Random.Range(0, 9);
        }
        else
        if (IncludeDiagonal)
        {
            return (Direction)UnityEngine.Random.Range(0, 8);
        }
        else
        {
            return (Direction)UnityEngine.Random.Range(0, 4);
        }
    }

    public static List<TileData> GetMapBorderFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.U:
                return GetAllMapTiles().Where(t => t.y == 63).ToList();
                break;

            case Direction.D:
                return GetAllMapTiles().Where(t => t.y == 0).ToList();
                break;

            case Direction.R:
                return GetAllMapTiles().Where(t => t.x == 63).ToList();
                break;

            case Direction.L:
                return GetAllMapTiles().Where(t => t.x == 0).ToList();
                break;

            case Direction.UL:
                break;

            case Direction.UR:
                break;

            case Direction.DR:
                break;

            case Direction.DL:
                break;

            case Direction.Null:
                break;

            default:
                break;
        }
        return null;
    }

    public static TileData GetTile(Func<TileData, bool> predicate)
    {
        return GetTiles(predicate).RandomItem();
    }

    public static List<TileData> GetTiles(Func<TileData, bool> predicate)
    {
        return LocationMap.MapRefference.Value.blobArray.ToArray().Where(predicate).ToList();
    }

    public static TileData GetFreeTile()
    {
        return GetTile(t =>
        t.SolidLayer == Entity.Null
        &&
        t.Template.SolidLayer == SimpleObjectName.Null
        &&
        !t.isAbyss
        &&
        !t.Template.doNotIncludeInRegions);

        // && (t.template.FloorLayer != ItemName.Null || t.SolidLayer != Entity.Null));
    }

    public static TileData GetNearestTileFromList(List<TileData> tilesList, TileData toTile)
    {
        float dst = 0;
        TileData tile = new TileData();
        for (int i = 0; i < tilesList.Count; i++)
        {
            float newDst = tilesList[i].GetSqrDistance(toTile);
            if (i == 0 || newDst < dst)
            {
                dst = newDst;
                tile = tilesList[i];
            }
        }
        return tile;
    }

    public static HashSet<TileData> GetTilesInRadius(TileData center, int radius)
    {
        radius++;
        HashSet<TileData> tiles = new HashSet<TileData>();
        for (int x = center.position.x - radius; x < center.position.x + radius; x++)
        {
            for (int y = center.position.y - radius; y < center.position.y + radius; y++)
            {
                if ((new int2(x, y) - center.position).SqrMagnitude() < Math.Pow(radius, 2))
                {
                    TileData newTile = new int2(x, y).ToTileData();
                    if (newTile != TileData.Null) tiles.Add(newTile);
                }
            }
        }
        return tiles;
    }

    public static void GetTilesInRadiusNative(TileData center, int radius, NativeArray<TileData> map, NativeList<TileData> result)
    {
        radius++;
        float sqrRadius = Mathf.Pow(radius, 2);
        float sqrDiameter = Mathf.Pow(radius * 2f, 2f);

        int xEnd = center.position.x + radius;
        int yEnd = center.position.y + radius;

        for (int x = center.position.x - radius; x < xEnd; x++)
        {
            for (int y = center.position.y - radius; y < yEnd; y++)
            {
                if ((new int2(x, y) - center.position).SqrMagnitude() < sqrRadius)
                {
                    var index = new int2(x, y).ToMapIndex();
                    var newTile = index.ToTileData(map);

                    if (newTile != TileData.Null) result.Add(newTile);
                }
            }
        }
    }

    public static TileData[] GetAllMapTiles()
    {
        return LocationMap.MapRefference.Value.blobArray.ToArray();
    }

    public static HashSet<TileData> GetTilesInRectangle(TileData leftCorner, TileData rightCorner)
    {
        if (leftCorner.valid && rightCorner.valid)
        {
            HashSet<TileData> tiles = new HashSet<TileData>();
            for (int x = leftCorner.x; x < rightCorner.x; x++)
            {
                for (int y = leftCorner.y; y < rightCorner.y; y++)
                {
                    TileData tile = new int2(x, y).ToTileData();
                    tiles.Add(tile);
                }
            }
            return tiles;
        }
        else
        {
            throw new ArgumentException("One of the corners is invalid : " + leftCorner + " " + rightCorner);
        }
    }

    public static TileData[,] GetTilesInRectangle2D(TileData leftCorner, TileData rightCorner)
    {
        if (leftCorner.valid && rightCorner.valid)
        {
            TileData[,] tiles = new TileData[rightCorner.x - leftCorner.x, rightCorner.y - leftCorner.y];
            for (int x = leftCorner.x; x < rightCorner.x; x++)
            {
                for (int y = leftCorner.y; y < rightCorner.y; y++)
                {
                    tiles[x - leftCorner.x, y - leftCorner.y] = new int2(x, y).ToTileData();
                }
            }
            return tiles;
        }
        else
        {
            throw new ArgumentException("One of the corners is invalid : " + leftCorner + " " + rightCorner);
        }
    }

    public static HashSet<TileData> GetTilesInRectangle(int2 leftCorner, int2 rightCorner)
    {
        HashSet<TileData> tiles = new HashSet<TileData>();
        for (int x = leftCorner.x; x < rightCorner.x; x++)
        {
            for (int y = leftCorner.y; y < rightCorner.y; y++)
            {
                tiles.Add(new int2(x, y).ToTileData());
            }
        }

        return tiles;
    }

    public static List<TileData> GetTilesInRectangleList(int2 leftCorner, int2 rightCorner)
    {
        List<TileData> tiles = new List<TileData>();
        for (int x = leftCorner.x; x < rightCorner.x; x++)
        {
            for (int y = leftCorner.y; y < rightCorner.y; y++)
            {
                tiles.Add(new int2(x, y).ToTileData());
            }
        }

        return tiles;
    }

    public static List<TileData> GetTilesInRectangle(TileData center, int sizex, int sizey)
    {
        List<TileData> tiles = new List<TileData>();
        for (int x = center.position.x - sizex / 2; x < center.position.x + sizex / 2; x++)
        {
            for (int y = center.position.y - sizey / 2; y < center.position.y + sizey / 2; y++)
            {
                TileData tile = new int2(x, y).ToTileData();
                if (tile != null) tiles.Add(tile);
            }
        }

        return tiles;
    }

    public static Direction GetOpositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.U:
                return Direction.D;

            case Direction.R:
                return Direction.L;

            case Direction.L:
                return Direction.R;

            case Direction.D:
                return Direction.U;
        }
        return Direction.U;
    }

    public static void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
}