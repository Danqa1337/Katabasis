using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class StructureBuilder
{
    public static bool FindStructure(TileData leftCorner, StructureName name)
    {
        var structure = StructuresDatabase.GetStructureData(name);
        TileData rightCorner = leftCorner + new int2(structure.Width, structure.Height);


        for (int x = 0; x < structure.Width; x++)
        {
            for (int y = 0; y < structure.Height; y++)
            {
                var tile = new int2(leftCorner.x + x, leftCorner.y + y).ToTileData();
                var template = structure.Map[new int2(x, y).ToMapIndex(structure.Width, structure.Height)];
                var SolidLayerFilter = new EnumFilter<SimpleObjectName>(new List<SimpleObjectName>() { template.SolidLayer });

                if (!SolidLayerFilter.Allows(tile.SolidLayer.GetItemName()))
                {
                    Debug.Log(name + " not found on " + leftCorner);
                    return false;
                }


                if (template.DropLayer != SimpleObjectName.Null && tile.DropLayer.Length > 0)
                {
                    bool dropFound = false;
                    foreach (var item in tile.DropLayer)
                    {
                        if (item.GetItemName() == template.DropLayer)
                        {
                            dropFound = true;
                            break;
                        }
                    }
                    if (dropFound == false)
                    {
                        Debug.Log(name + " not found on " + leftCorner);
                        return false;
                    }
                }




            }

        }


        Debug.Log(name + " found on " + leftCorner);
        return true;


    }
    public static StructureData TryToFitStructure(StructureData structure, TileData leftCorner, Predicate<TileData[,]> rectanglePredicate = null, Predicate<(TileTemplate, TileData)> tilePredicate = null)
    {


        if (BaseMethodsClass.Chance(50))
        {
            structure = structure.Turn();
        }
        if (BaseMethodsClass.Chance(50))
        {
            structure = structure.Mirror();
        }

        if (CheckFreeSpaceForStructure(leftCorner, structure, rectanglePredicate, tilePredicate))
        {

            return BuildStructure(leftCorner, structure);

        }
        else
        {
            return null;
        }


    }
    public static StructureData BuildStructure(TileData leftCorner, StructureData structureData)
    {
        var generatedStructure = structureData.DeepClone<StructureData>();
        for (int x = 0; x < structureData.Width; x++)
        {
            for (int y = 0; y < structureData.Height; y++)
            {
                int X = leftCorner.x + x;
                int Y = leftCorner.y + y;
                var localMapIndex = new int2(x, y).ToMapIndex(structureData.Width, structureData.Height);

                var template = structureData.Map[localMapIndex];
                if (template != TileTemplate.Null)
                {

                    template.index = new int2(X, Y).ToMapIndex();
                    generatedStructure.Map[localMapIndex] = template;
                    template.Save();

                }
            }
        }
        var spawnedCreatures = new List<Entity>();
        for (int i = 0; i < structureData.spawnPoints.Length; i++)
        {
            if (structureData.CreatureNames[i] != ComplexObjectName.Null)
            {
                if (!structureData.spawnPoints[i].Equals(new int2(-1)))
                {
                    spawnedCreatures.Add((structureData.spawnPoints[i] + leftCorner.position).ToTileData().Spawn(structureData.CreatureNames[i]));
                }
            }
        }
        if (spawnedCreatures.Count > 1 && Application.isPlaying)
        {
            Debug.Log("!");
            var mainSquadIndex = spawnedCreatures[0].GetComponentData<SquadMemberComponent>().squadIndex;
            for (int i = 1; i < spawnedCreatures.Count; i++)
            {
                Registers.SquadsRegister.MoveToSquad(mainSquadIndex, spawnedCreatures[i]);
            }
        }

        return generatedStructure;
    }
    public static void BuildStructure(TileData leftCorner, StructureName name, bool turnRooms = true)
    {
        StructureData structure = StructuresDatabase.GetStructureData(name);

        BuildStructure(leftCorner, structure);



    }


    public static bool CheckFreeSpaceForStructure(TileData leftCorner, StructureData structure, Predicate<TileData[,]> rectanglePredicate = null, Predicate<(TileTemplate, TileData)> tilePredicate = null)
    {
        var rightCorner = leftCorner + new int2(structure.Width, structure.Height);

        if (leftCorner.valid && rightCorner.valid)
        {
            var rectangle = BaseMethodsClass.GetTilesInRectangle2D(leftCorner, rightCorner);
            if (true) //rectanglePredicate != null && rectanglePredicate(rectangle))
            {
                var width = rectangle.GetLength(0);
                var height = rectangle.GetLength(1);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var mapTile = rectangle[x, y];
                        var structureTemplate = structure.Map[new int2(x, y).ToMapIndex(width, height)];

                        var matchingStructurePosition = (mapTile - leftCorner);
                        var mathcingStructureTileTemplate = structure.Map[new int2(matchingStructurePosition.x, matchingStructurePosition.y).ToMapIndex(structure.Width, structure.Height)];

                        if ((!mapTile.valid
                            ||
                            mapTile.isBorderTile())
                            ||
                            (!structure.TemplateStateFilter.Allows(mapTile.Template.templateState) && (mapTile.Template != mathcingStructureTileTemplate))
                            ||
                            !(tilePredicate == null || tilePredicate((structureTemplate, mapTile))))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
        return false;
    }
    public static bool CheckFreeSpaceForStructure(TileData leftCorner, StructureName name)
    {
        StructureData structure = StructuresDatabase.GetStructureData(name);
        return CheckFreeSpaceForStructure(leftCorner, structure);
    }
}
