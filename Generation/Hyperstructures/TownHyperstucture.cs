using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TownHyperstucture : Hyperstructure
{
    public List<StructureName> structureNames = new List<StructureName>()
    {
       StructureName.Store1,
       StructureName.SlaveStore1,
       StructureName.Hut1,
       StructureName.Hut2,
       StructureName.Hut3,
       StructureName.Hut4,
       StructureName.Hut5,
       StructureName.BushFarm1,
       StructureName.MushroomFarm1,
       StructureName.SmallWell,
       StructureName.Statue1,
    };

    public TileData[,] Map => BaseMethodsClass.GetTilesInRectangle2D((center - new int2(radius, radius).ToTileData()).ToTileData(), (center + new int2(radius, radius).ToTileData()).ToTileData());
    public int roomNum = 35;
    public int radius = 24;
    public TileData center;

    public override void Generate(TileData center)
    {
        generatedStrctures = new List<StructureData>();
        this.center = center;
        GenerateCave();
        BuildRooms();
        ConnectBuildings();
    }

    public void BuildRooms()
    {
        var structureNames = new List<string>();
        var structureList = new List<StructureData>();

        for (int i = 0; i < roomNum; i++)
        {
            var structure = StructuresDatabase.GetStructureData(this.structureNames.RandomItem());

            structureList.Add(structure);
        }
        structureList.Shuffle(); //OrderBy(r => 64-r.GetSize()).ToList();
        Debug.Log(structureList.Count + " rooms");

        for (int i = 0; i < roomNum; i++)
        {
            var freeTiles = BaseMethodsClass.GetTilesInRadius(center, 10).ToList();
            var leftCorner = freeTiles.RandomItem();

            for (int t = 0; t < structureList.Count; t++)
            {
                Predicate<(TileTemplate, TileData)> tilePredicate = pair => (!pair.Item2.CheckStateInNeibors(TemplateState.Door, false))
                    && ((pair.Item1.templateState != TemplateState.Door || (!pair.Item2.CheckStateInNeibors(TemplateState.Wall, false) && !pair.Item2.CheckStateInNeibors(TemplateState.Darkness, false))));

                var newBuiltStructure = StructureBuilder.TryToFitStructure(structureList[t], leftCorner, tilePredicate: tilePredicate);

                if (newBuiltStructure != null)
                {
                    generatedStrctures.Add(newBuiltStructure);
                    for (int m = 0; m < newBuiltStructure.Map.Length; m++)
                    {
                        if (newBuiltStructure.Map[m] != TileTemplate.Null)
                        {
                            var template = newBuiltStructure.Map[m];
                            template.doNotIncludeInRegions = true;

                            template.Save();
                        }
                    }
                    break;
                }
            }
            structureList.Shuffle();
        }
    }

    private void GenerateCave()
    {
        var automaton = CelularAutomaton.GenerateMap(radius * 2, 30, 3, 40, 1, 1, 0.05f);
        var map = Map;

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (automaton[x, y] == 0)
                {
                    var template = map[x, y].Template;
                    template.SolidLayer = SimpleObjectName.Null;
                    template.FloorLayer = SimpleObjectName.RockFloor;
                    template.SetState(TemplateState.Floor);
                    template.SetBiome(Biome.Town);
                }
                else
                {
                }
            }
        }
    }

    private void ConnectBuildings()
    {
        if (generatedStrctures.Count > 1)
        {
            var arcs = new List<TileTemplate>();
            foreach (var structure in generatedStrctures)
            {
                foreach (var template in structure.Map)
                {
                    if (template.isCoridorStart)
                    {
                        arcs.Add(template);
                    }
                }
            }

            foreach (var structure in generatedStrctures)
            {
                var localArcs = structure.Map.Where(t => t.isCoridorStart).ToList();
                foreach (var firstArc in localArcs)
                {
                    var otherArcs = arcs.Where(a => !localArcs.Contains(a)).ToList();
                    if (otherArcs.Count > 0)
                    {
                        var secondArc = otherArcs.OrderBy(a => (a.index.ToMapPosition() - firstArc.index.ToMapPosition()).SqrMagnitude()).ToList()[0];
                        var path = Pathfinder.FindPathNoJob(firstArc.index.ToMapPosition(), secondArc.index.ToMapPosition(), (t => t.Template.SolidLayer == SimpleObjectName.Null ? 1 : -1), tryLimit: 1000);
                        Debug.Log(String.Format("Building path form {0}, to {1} ", firstArc.index.ToTileData(), secondArc.index.ToTileData()));
                        Debug.Log(path);
                        foreach (var item in path.Nodes)
                        {
                            var tile = item.ToTileData();
                            if (tile.position.Equals(path.Nodes[0]))
                            {
                                if (tile.Template.templateState != TemplateState.Door)
                                {
                                    var template = tile.Template;
                                    template.doNotIncludeInRegions = true;
                                    template.Save();

                                    tile.Template.ClearLayer(ObjectType.Solid);
                                    tile.Template.GenerateObject(SimpleObjectName.RockFloor);
                                    tile.Template.GenerateObject(SimpleObjectName.DirtPath);
                                }
                            }
                        }
                        if (path != PathFinderPath.Null && firstArc.templateState != TemplateState.Door)
                        {
                            firstArc.GenerateObject(SimpleObjectName.Door);
                        }
                        path.Dispose();
                    }
                }
            }
        }
    }
}