using Locations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;

public abstract class GenerationModule : MonoBehaviour
{
    [SerializeField] private bool _debug;
    protected bool Debug => _debug;

    protected bool Chance(float percent) => KatabasisUtillsClass.Chance(percent);

    public abstract void Generate(GenerationData generationData);

    protected int GetFloorTilesNum()
    {
        return BaseMethodsClass.GetAllMapTiles().Where(t => t.Template.SolidLayer == SimpleObjectName.Null).ToArray().Length;
    }

    protected TileData GetFreeTile()
    {
        return BaseMethodsClass.GetFreeTile();
    }

    public static TileData[] GetAllMapTiles()
    {
        return BaseMethodsClass.GetAllMapTiles();
    }

    public static TileData GetTile(Func<TileData, bool> predicate)
    {
        return BaseMethodsClass.GetTile(predicate);
    }

    public static List<TileData> GetTiles(Func<TileData, bool> predicate)
    {
        return BaseMethodsClass.GetTiles(predicate);
    }

    protected void ExpandPassage(PathFinderPath path, float expandingChance)
    {
        foreach (var tile in path.waySide)
        {
            var startNeibors = path.StartPosition.ToTileData().GetNeibors(false);
            var endNeibors = path.EndPosition.ToTileData().GetNeibors(false);

            if (Chance(expandingChance))
            {
                if (tile.valid && !tile.isBorderTile())
                {
                    if (tile.Template.templateState == TemplateState.Darkness)
                    {
                        path.Add(tile.position);
                    }
                }
            }
        }
    }

    protected void TurnPathIntoBrickCoridor(PathFinderPath path)
    {
        if (path != PathFinderPath.Null)
        {
            foreach (var item in path.waySide.Where(t => t.Template.templateState == TemplateState.Darkness))
            {
                var template = item.Template;
                template.isCoridorStart = false;
                template.GenerateObject(SimpleObjectName.BrickWall);
            }

            foreach (var item in path.Nodes)
            {
                item.ToTileData().Template.Clear();
                item.ToTileData().Template.GenerateObject(SimpleObjectName.BrickFloor);
                item.ToTileData().Template.SetBiome(Biome.Dungeon);
            }
        }
    }

    protected void TurnWayIntoMarbleCoridor(PathFinderPath way)
    {
        foreach (var item in way.waySide.Where(t => t.Template.templateState == TemplateState.Darkness))
        {
            item.Template.Clear();
            item.Template.GenerateObject(SimpleObjectName.MarbleWall);
        }
        foreach (var item in way.Nodes)
        {
            item.ToTileData().Template.Clear();
            item.ToTileData().Template.GenerateObject(SimpleObjectName.MarbleFloor);
            item.ToTileData().Template.SetBiome(Biome.Dungeon);
        }
    }

    protected void TurnPathIntoCavePassage(PathFinderPath way)
    {
        foreach (var item in way.Nodes)
        {
            var template = item.ToTileData().Template;
            template.Clear();
            template.GenerateObject(SimpleObjectName.RockFloor);
        }
    }

    private void OnEnable()
    {
        //needed for enabled checkbox to appear
    }
}