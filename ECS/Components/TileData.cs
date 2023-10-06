using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct TileData : IEquatable<TileData>, IComponentData
{
    public int index;
    public float lightLevel;
    public float dstToPlayer;

    public bool visible;
    public bool maped;
    public bool visionBlocked;
    public bool hasCreature;
    public bool isPlayersTile;
    public bool hasActivePressurePlate;
    public bool isAltar;
    public bool isOnFire;
    public int transitionId;

    [NonSerialized] public Entity SolidLayer;
    [NonSerialized] public Entity LiquidLayer;
    [NonSerialized] public Entity FloorLayer;
    [NonSerialized] public Entity HoveringLayer;

    public Biome biome;

    public TileTemplate Template
    {
        get
        {
            if (LocationMap.TemplateMap != null)
            {
                if (this != Null)
                {
                    return LocationMap.TemplateMap[index];
                }
                else
                {
                    throw new NullTileException();
                }
            }
            else
            {
                return TileTemplate.Null;
            }
        }
        set
        {
            if (this != Null)
            {
                LocationMap.TemplateMap[index] = value;
            }
            else
            {
                throw new NullTileException();
            }
        }
    }

    public readonly Entity[] DropLayer
    {
        get
        {
            if (this == Null) throw new NullTileException();
            return LocationMap.TileDropContainers[index].ToArray();
        }
    }

    public readonly Entity[] GroundCoverLayer
    {
        get
        {
            if (this == Null) throw new NullTileException();
            return LocationMap.TileGroundCoverContainers[index].ToArray();
        }
    }

    public static TileData Null
    {
        get
        {
            return new TileData { index = -1, transitionId = -1 };
        }
    }

    public int2 position => index.ToMapPosition();
    public int x => position.x;
    public int y => position.y;

    public bool valid
    {
        get
        {
            if (this == Null || position.x < 0 || position.y < 0 || position.x > 63 || position.y > 63)
            {
                return false;
            }
            return true;
        }
    }

    public bool isAbyss => FloorLayer == Entity.Null;

    public void Add(Entity entity, ObjectType objectType)
    {
        if (entity.HasComponent<PressurePlateComponent>())
        {
            hasActivePressurePlate = true;
        }
        switch (objectType)
        {
            case ObjectType.Solid:
                SolidLayer = entity;
                visionBlocked = entity.HasComponent<LOSBlockTag>();
                break;

            case ObjectType.Drop:
                Drop(entity);
                break;

            case ObjectType.Floor:
                FloorLayer = entity;
                break;

            case ObjectType.Liquid:
                LiquidLayer = entity;
                break;

            case ObjectType.GroundCover:
                LocationMap.TileGroundCoverContainers[index].Add(entity);
                break;

            case ObjectType.Hovering:
                HoveringLayer = entity;
                break;
        }
        Save();
    }

    public void Remove(Entity entity)
    {
        if (SolidLayer == entity)
        {
            SolidLayer = Entity.Null;
            visionBlocked = false;
        }
        if (DropLayer.Contains(entity)) LocationMap.TileDropContainers[index].Remove(entity);
        if (GroundCoverLayer.Contains(entity)) LocationMap.TileGroundCoverContainers[index].Remove(entity);
        if (FloorLayer == entity) FloorLayer = Entity.Null;
        if (HoveringLayer == entity) HoveringLayer = Entity.Null;
        if (LiquidLayer == entity) LiquidLayer = Entity.Null;
        Save();
    }

    public void Drop(Entity item)
    {
        var tr = item.GetComponentObject<EntityAuthoring>().transform;
        item.GetComponentObject<RendererComponent>().transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(90f, 270f));

        if (!DropLayer.Contains(item))
        {
            LocationMap.TileDropContainers[index].Add(item);
            var cr = item.GetComponentData<CurrentTileComponent>();
            var objectTypeComponent = item.GetComponentData<ObjectTypeComponent>();

            if (item.HasComponent<AnatomyComponent>())
            {
                foreach (var part in item.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
                {
                    part.GetComponentObject<RendererComponent>().spritesSortingLayer = "Drop";
                }
            }

            tr.SetParent(null);
            var renderer = item.GetComponentObject<RendererComponent>();
            // renderer.transform.localPosition = renderer.spriteCenterOffset;
            //tr.rotation = quaternion.Euler(0, 0, UnityEngine.Random.Range(90, 270));
            tr.position = position.ToRealPosition(item)  /*(Vector3) renderer.spriteCenterOffset*/ + new Vector3(0, -0.2f, 0);

            cr.currentTileId = index;

            objectTypeComponent.objectType = ObjectType.Drop;
            item.SetComponentData(cr);
            item.SetComponentData(objectTypeComponent);
            //Debug.Log(item.GetName() + " dropped on " + position);
            Save();
        }
    }

    public Entity Spawn(SimpleObjectName itemName)
    {
        return Spawner.Spawn(itemName, this);
    }

    public Entity Spawn(SimpleObjectData objectData)
    {
        return Spawner.Spawn(objectData, this);
    }

    public Entity Spawn(ComplexObjectName itemName)
    {
        return Spawner.Spawn(itemName, this);
    }

    public Entity Spawn(ComplexObjectData objectData)
    {
        return Spawner.Spawn(objectData, this);
    }

    public void ApplyTemplate()
    {
        if (Template.FloorLayer != SimpleObjectName.Null && Template.FloorLayer != SimpleObjectName.Any) Spawner.Spawn(Template.FloorLayer, this);
        if (Template.SolidLayer != SimpleObjectName.Null && Template.SolidLayer != SimpleObjectName.Any) Spawner.Spawn(Template.SolidLayer, this);
        if (Template.DropLayer != SimpleObjectName.Null && Template.DropLayer != SimpleObjectName.Any) Spawner.Spawn(Template.DropLayer, this);
        if (Template.LiquidLayer != SimpleObjectName.Null && Template.LiquidLayer != SimpleObjectName.Any) Spawner.Spawn(Template.LiquidLayer, this);
        if (Template.GroundCover != SimpleObjectName.Null && Template.GroundCover != SimpleObjectName.Any) Spawner.Spawn(Template.GroundCover, this);
        if (Template.HoveringLayer != SimpleObjectName.Null && Template.HoveringLayer != SimpleObjectName.Any) Spawner.Spawn(Template.HoveringLayer, this);
        this.biome = Template.Biome;
        Template = TileTemplate.Null;
        Save();
    }

    public HashSet<RendererComponent> GetAllRenderers()
    {
        var renderers = new HashSet<RendererComponent>();
        var objects = new HashSet<Entity>();

        if (SolidLayer != Entity.Null)
        {
            foreach (var item in SolidLayer.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
            {
                objects.Add(item);
            }
        }
        if (LiquidLayer != Entity.Null)
        {
            objects.Add(LiquidLayer);
        }
        if (HoveringLayer != Entity.Null)
        {
            objects.Add(HoveringLayer);
        }
        if (FloorLayer != Entity.Null)
        {
            objects.Add(FloorLayer);
        }
        foreach (var item in DropLayer)
        {
            foreach (var part in item.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
            {
                objects.Add(part);
            }
        }
        foreach (var item in GroundCoverLayer)
        {
            objects.Add(item);
        }
        foreach (var item in objects)
        {
            renderers.Add(item.GetComponentObject<RendererComponent>());
        }

        return renderers;
    }

    public void ShowUnmapable()
    {
        if (SolidLayer.HasComponent<AIComponent>())
        {
            SolidLayer.ShowRenderer();
        }
        if (HoveringLayer.HasComponent<AIComponent>())
        {
            HoveringLayer.ShowRenderer();
        }
        foreach (var item in DropLayer)
        {
            item.ShowRenderer();
        }
    }

    public void HideUnmapable()
    {
        if (SolidLayer.HasComponent<AIComponent>())
        {
            SolidLayer.HideRenderer();
        }
        if (HoveringLayer.HasComponent<AIComponent>())
        {
            HoveringLayer.HideRenderer();
        }
        foreach (var item in DropLayer)
        {
            item.HideRenderer();
        }
    }

    public void Save()
    {
        LocationMap.SetTileData(this);
    }

    public void Save(NativeArray<TileData> array)
    {
        array[index] = this;
    }

    public TileData Refresh()
    {
        return LocationMap.GetTileData(index);
    }

    public bool isInsideCameraFrustrum()
    {
        var frustrum = MainCameraHandler.MainCamera.WorldToViewportPoint(position.ToRealPosition(Entity.Null));
        if (frustrum.x > 1 || frustrum.x < 0 || frustrum.y > 1 || frustrum.y < 0)
        {
            return false;
        }
        return true;
    }

    [GenerateTestsForBurstCompatibility]
    public bool isWalkableBurstSafe(WalkabilityDataComponent walkabilityData, int2 currentPosition)
    {
        if (!valid)
        {
            return false;
        }
        if (isOnFire)
        {
            return false;
        }

        if (!walkabilityData.isPlayer || (walkabilityData.isPlayer && maped)) //is not player or tile maped
        {
            if (walkabilityData.hovering) //is hovering object
            {
                return true;
            }
            else
            {
                if (SolidLayer == Entity.Null) //is free
                {
                    if (isAbyss) //tile has no floor
                    {
                        if (walkabilityData.flying)//but creature can fly
                        {
                            return true;
                        }
                    }
                    else // tile has floor
                    {
                        return true;
                    }
                }
                else //is not free
                {
                    if (hasCreature) // solid is creature
                    {
                        if ((position - currentPosition).SqrMagnitude() > 4)  //it is not neibor tile
                        {
                            return true;
                        }
                        else
                        {
                            if (walkabilityData.isPlayer)
                            {
                                return true;
                            }

                            if (walkabilityData.isPlayersSquadmate && !isPlayersTile)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (walkabilityData.digger)// creature can dig
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool IsInRangeOfOne(TileData tileData)
    {
        return this.GetSqrDistance(tileData) < 4 && tileData != this;
    }

    public bool IsInRangeOfTwo(TileData tileData)
    {
        return this.GetSqrDistance(tileData) < 9 && tileData != this;
    }

    public bool isWalkable(Entity creature)
    {
        return isWalkableBurstSafe(creature.GetComponentData<WalkabilityDataComponent>(), creature.CurrentTile().position);
    }

    public TileData[] GetNeibors(bool diagonals)
    {
        TileData[] neibors;
        if (diagonals)
        {
            neibors = new TileData[8];
            for (int i = 0; i < 8; i++)
            {
                neibors[i] = (position + LocationMap.NeiborsOffsetsArray8[i]).ToTileData();
            }
        }
        else
        {
            neibors = new TileData[4];
            for (int i = 0; i < 4; i++)
            {
                neibors[i] = (position + LocationMap.NeiborsOffsetsArray8[i]).ToTileData();
            }
        }

        return neibors;
    }

    public NativeArray<TileData> GetNeiborsSafe(bool diagonals)
    {
        if (diagonals)
        {
            var neibors = new NativeArray<TileData>(8, Allocator.Temp);
            for (int i = 0; i < 8; i++)
            {
                neibors[i] = (position + LocationMap.NeiborsOffsetsArray8[i]).ToTileData();
            }
            return neibors;
        }
        else
        {
            var neibors = new NativeArray<TileData>(4, Allocator.Temp);
            for (int i = 0; i < 4; i++)
            {
                neibors[i] = (position + LocationMap.NeiborsOffsetsArray8[i]).ToTileData();
            }
            return neibors;
        }
    }

    public TileData[] GetNeiborsShuffle(bool diagonals)
    {
        return GetNeibors(diagonals).Shuffle().ToArray();
    }

    public HashSet<TileData> GetTilesInRadius(int radius)
    {
        return BaseMethodsClass.GetTilesInRadius(this, radius);
    }

    public bool CheckStateInNeibors(TemplateState _state, bool includeDiagonals, int numberOfNaiborsWithState = 1)
    {
        int n = 0;
        foreach (TileData neibor in GetNeibors(includeDiagonals))
        {
            if (neibor != TileData.Null)
            {
                if (neibor.Template.templateState == _state) n++;
                if (n == numberOfNaiborsWithState) return true;
            }
        }
        return false;
    }

    public bool checkDirectionState(Direction direction, TemplateState state)
    {
        if (direction == Direction.Null)
        {
            if (Template.templateState == state) return true;
            return false;
        }

        if ((this + direction) != TileData.Null && (this + direction).Template.templateState == state)
        {
            //Debug.Log("tile on " + (tile.position + getVectorFromDirection(direction)) + " is " + state);
            return true;
        }
        else
        {
            // Debug.Log("tile on " + (tile.position + getVectorFromDirection(direction)) + " is not " + state);
            return false;
        }
    }

    public bool isBorderTile()
    {
        if (position.x == 0 || position.y == 0 || position.x == 63 || position.y == 63)
        {
            return true;
        }
        return false;
    }

    public bool isPlaceForDoor()
    {
        if (Template.templateState == TemplateState.Floor || Template.templateState == TemplateState.Door)
        {
            if ((this + Direction.L).Template.templateState == TemplateState.Wall && (this + Direction.R).Template.templateState == TemplateState.Wall)
            {
                return true;
            }
            else if ((this + Direction.U).Template.templateState == TemplateState.Wall && (this + Direction.D).Template.templateState == TemplateState.Wall)
            {
                return true;
            }
        }
        return false;
    }

    public int GetDistanceFromEdge()
    {
        return Mathf.Min(x, 64 - x, y, 64 - y);
    }

    public static TileData operator +(TileData tileData, int2 vector)
    {
        return (tileData.position + vector).ToTileData();
    }

    public static TileData operator -(TileData tileData, int2 vector)
    {
        return (tileData.position - vector).ToTileData();
    }

    public static int2 operator +(TileData A, TileData B)
    {
        return A.position + B.position;
    }

    public static TileData operator +(TileData A, Direction direction)
    {
        switch (direction)
        {
            case Direction.U:
                return A + new int2(0, 1);

            case Direction.R:
                return A + new int2(1, 0);

            case Direction.L:
                return A + new int2(-1, 0);

            case Direction.D:
                return A + new int2(0, -1);

            case Direction.DL:
                return A + new int2(-1, -1);

            case Direction.DR:
                return A + new int2(1, -1);

            case Direction.UL:
                return A + new int2(-1, 1);

            case Direction.UR:
                return A + new int2(1, 1);

            case Direction.Null:
                return A + new int2(0, 0);
        }
        return TileData.Null;
    }

    public static int2 operator -(TileData A, TileData B)
    {
        return A.position - B.position;
    }

    public static bool operator ==(TileData A, TileData B)
    {
        return A.index == B.index;
    }

    public static bool operator !=(TileData A, TileData B)
    {
        return A.index != B.index;
    }

    public bool ClearLineOfSight(TileData targetTile, out float distance)
    {
        if (targetTile == Null)
        {
            throw new Exception("lineOfSight target tile is null");
        }
        if (this == Null)
        {
            throw new Exception("lineOfSight start tile is null");
        }
        if (targetTile == this)
        {
            distance = 0;
            return true;
        }

        float2 raycastFloatPos = position;
        TileData currentRaycastTile;
        float2 raycastDirection = math.normalize(targetTile.position - position);

        distance = 0f;
        int testIndex = 0;
        int t = 0;
        while (true)
        {
            t++;

            if (t > 100)
            {
                throw new Exception("endless clear line of sight search bug");
            }
            distance++;
            raycastFloatPos += raycastDirection;
            currentRaycastTile = new int2((int)(raycastFloatPos.x + 0.5f), (int)(raycastFloatPos.y + 0.5f)).ToTileData();

            if (testIndex == -1 || currentRaycastTile == targetTile) return true;
            if (currentRaycastTile.visionBlocked)
            {
                return false;
            }
        }
    }

    public override string ToString()
    {
        if (this == Null) return "Tile Null";
        else return String.Format("Tile {0}, {1}", position.x, position.y);
    }

    public bool ClearTraectory(TileData tile2)
    {
        if (tile2 != TileData.Null)
        {
            float2 raycastFloatPos = position;
            TileData currentRaycastTile;
            float2 raycastDirection = math.normalize(tile2.position - position);

            int testIndex = 0;
            while (true)
            {
                raycastFloatPos += raycastDirection;
                currentRaycastTile = new int2((int)(raycastFloatPos.x + 0.5f), (int)(raycastFloatPos.y + 0.5f)).ToTileData();

                if (testIndex == -1 || currentRaycastTile == tile2) return true;
                if (currentRaycastTile.SolidLayer != Entity.Null)
                {
                    //if(currentRaycastTile != this && currentRaycastTile != tile2)
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }

    public bool ClearLineOfSight(TileData tile2)
    {
        float f;
        return ClearLineOfSight(tile2, out f);
    }

    public bool Equals(TileData other)
    {
        return index == other.index;
    }
}

public class NullTileException : Exception
{
    public override string ToString()
    {
        return "Null tile exception: You are trying to access Null tile data. It is not allowed";
    }
}