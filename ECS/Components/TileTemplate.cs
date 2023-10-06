using UnityEngine;

[System.Serializable]
public struct TileTemplate
{
    public int index;
    public SimpleObjectName SolidLayer;
    public SimpleObjectName DropLayer;
    public SimpleObjectName LiquidLayer;
    public SimpleObjectName FloorLayer;
    public SimpleObjectName GroundCover;
    public SimpleObjectName HoveringLayer;
    public TemplateState templateState;
    public Biome Biome;

    public bool doNotIncludeInRegions;
    public bool isCoridorStart;
    public bool isCoridorBlock;

    public static TileTemplate Null =>
    new TileTemplate
    {
        index = -1,
        SolidLayer = SimpleObjectName.Null,
        DropLayer = SimpleObjectName.Null,
        LiquidLayer = SimpleObjectName.Null,
        GroundCover = SimpleObjectName.Null,
        HoveringLayer = SimpleObjectName.Null,
        FloorLayer = SimpleObjectName.Null,
        templateState = TemplateState.Darkness,

        Biome = Biome.Cave,
        isCoridorBlock = false,
        isCoridorStart = false,
    };

    public TileTemplate(int index)
    {
        this.index = index;
        SolidLayer = SimpleObjectName.Null;
        DropLayer = SimpleObjectName.Null;
        LiquidLayer = SimpleObjectName.Null;
        GroundCover = SimpleObjectName.Null;
        HoveringLayer = SimpleObjectName.Null;
        FloorLayer = SimpleObjectName.RockFloor;
        templateState = TemplateState.Darkness;
        Biome = Biome.Cave;

        doNotIncludeInRegions = false;
        isCoridorBlock = false;
        isCoridorStart = false;
    }

    public static bool operator ==(TileTemplate A, TileTemplate B)
    {
        return A.SolidLayer == B.SolidLayer &&
               A.FloorLayer == B.FloorLayer &&
               A.GroundCover == B.GroundCover &&
               A.LiquidLayer == B.LiquidLayer &&
               A.DropLayer == B.DropLayer &&
               A.HoveringLayer == B.HoveringLayer;
    }

    public static bool operator !=(TileTemplate A, TileTemplate B)
    {
        return A.SolidLayer != B.SolidLayer ||
       A.FloorLayer != B.FloorLayer ||
       A.GroundCover != B.GroundCover ||
       A.LiquidLayer != B.LiquidLayer ||
       A.DropLayer != B.DropLayer ||
       A.HoveringLayer != B.HoveringLayer;
    }

    public void GenerateObject(SimpleObjectData data, bool save = true)
    {
        SetState(data.defaultTileState, false);
        switch (data.objectTypeComponent.objectType)
        {
            case ObjectType.Solid:
                this.SolidLayer = data.SimpleObjectName;
                break;

            case ObjectType.Drop:
                this.DropLayer = data.SimpleObjectName;
                break;

            case ObjectType.Floor:
                this.FloorLayer = data.SimpleObjectName;
                break;

            case ObjectType.Liquid:
                this.LiquidLayer = data.SimpleObjectName;
                break;

            case ObjectType.GroundCover:
                this.GroundCover = data.SimpleObjectName;
                break;

            case ObjectType.Hovering:
                this.HoveringLayer = data.SimpleObjectName;
                break;
        }
        if (save) Save();
    }

    public void Clear()
    {
        SolidLayer = SimpleObjectName.Null;
        DropLayer = SimpleObjectName.Null;
        LiquidLayer = SimpleObjectName.Null;
        GroundCover = SimpleObjectName.Null;
        HoveringLayer = SimpleObjectName.Null;
        FloorLayer = SimpleObjectName.RockFloor;
        templateState = TemplateState.Darkness;
        isCoridorBlock = false;
        isCoridorStart = false;
        doNotIncludeInRegions = false;
        Save();
    }

    public void ClearLayer(ObjectType objectType)
    {
        switch (objectType)
        {
            case ObjectType.Solid:
                SolidLayer = SimpleObjectName.Null;
                break;

            case ObjectType.Drop:
                DropLayer = SimpleObjectName.Null;
                break;

            case ObjectType.Floor:
                FloorLayer = SimpleObjectName.Null;
                break;

            case ObjectType.Liquid:
                LiquidLayer = SimpleObjectName.Null;
                break;

            case ObjectType.GroundCover:
                GroundCover = SimpleObjectName.Null;
                break;

            case ObjectType.Hovering:
                HoveringLayer = SimpleObjectName.Null;
                break;
        }
        Save();
    }

    public void Save()
    {
        LocationMap.TemplateMap[index] = this;
    }

    public void MakeAbyss()
    {
        SolidLayer = SimpleObjectName.Null;
        DropLayer = SimpleObjectName.Null;
        GroundCover = SimpleObjectName.Null;
        FloorLayer = SimpleObjectName.Null;
        HoveringLayer = SimpleObjectName.Null;
        templateState = TemplateState.Abyss;
        Save();
    }

    public void SetState(TemplateState templateState, bool save = true)
    {
        if (templateState != TemplateState.Any)
        {
            this.templateState = templateState;
            if (save) Save();
        }
    }

    public void SetBiome(Biome biome, bool save = true)
    {
        if (biome != Biome.Any)
        {
            this.Biome = biome;
            if (save) Save();
        }
    }

    public void GenerateObject(SimpleObjectName itemName, bool save = true)
    {
        var obj = SimpleObjectsDatabase.GetObjectData(itemName, true);
        GenerateObject(obj, save);
    }

    public bool isFloor()
    {
        return (templateState == TemplateState.Floor || templateState == TemplateState.Door || templateState == TemplateState.ShallowWater);
    }
}