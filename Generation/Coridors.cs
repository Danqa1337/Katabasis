using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;

namespace GenerationModules
{
    public class Coridors : GenerationModule
    {
        [SerializeField] private float _coridorTurnChance;

        public override void Generate(GenerationData generationData)
        {
            generationData.Arcs = GetTiles(t => t.Template.isCoridorStart);

            foreach (var arc in generationData.Arcs)
            {
                PathFinderPath path = GeneratePath(arc);
                if (path != PathFinderPath.Null)
                {
                    var startNeibors = path.StartPosition.ToTileData().GetNeibors(true);
                    var endNeibors = path.EndPosition.ToTileData().GetNeibors(true);

                    if (Chance(0) && path.Length >= 5 && path.StartPosition.GetDistance(path.EndPosition) > 4)
                    {
                        if (!path.waySide.Any(t => (t.Template.templateState != TemplateState.Darkness && !startNeibors.Contains(t) && !endNeibors.Contains(t))))
                        {
                            ExpandPassage(path, 100);
                        }
                    }

                    TurnPathIntoBrickCoridor(path);
                }
                path.Dispose();
            }

            PathFinderPath GeneratePath(TileData arc)
            {
                var pathTiles = new List<TileData>() { arc };

                if (arc.CheckStateInNeibors(TemplateState.Floor, false, 2))
                {
                    return new PathFinderPath(pathTiles, Allocator.Persistent);
                }

                var currentDirection = GetFirstDirection();
                var lastDirection = currentDirection;
                var currentTile = arc + currentDirection;
                var nextTile = currentTile + currentDirection;

                if (currentDirection != Direction.Null)
                {
                    pathTiles.Add(currentTile);

                    while (true)
                    {
                        if (currentTile.CheckStateInNeibors(TemplateState.Floor, false))
                        {
                            return new PathFinderPath(pathTiles);
                        }

                        if (itsTimeForForcedTurn() || itsTimeForSpantanTurn())
                        {
                            List<Direction> posibleDirections = new List<Direction>() { Direction.U, Direction.D, Direction.R, Direction.L };

                            posibleDirections.Remove(lastDirection);
                            posibleDirections.Remove(BaseMethodsClass.GetOpositeDirection(lastDirection));
                            posibleDirections.Remove(BaseMethodsClass.GetOpositeDirection(currentDirection));
                            posibleDirections.Remove(currentDirection);

                            currentDirection = posibleDirections.RandomItem();
                            lastDirection = currentDirection;
                        }

                        currentTile = currentTile + currentDirection;
                        nextTile = currentTile + currentDirection;
                        pathTiles.Add(currentTile);

                        if (nextTile.isBorderTile())
                        {
                            return PathFinderPath.Null;
                        }
                    }

                    bool itsTimeForSpantanTurn()
                    {
                        if (Chance(_coridorTurnChance) && pathTiles.Count > 3 && pathTiles.Count % 2 == 0)
                        {
                            return true;
                        }
                        return false;
                    }
                    bool itsTimeForForcedTurn()
                    {
                        if (nextTile.GetNeibors(false).Any(t => t != TileData.Null && t.Template.isCoridorStart && t != arc))
                        {
                            if (nextTile != TileData.Null && (!nextTile.Template.isCoridorBlock && nextTile.isBorderTile() || nextTile.Template.templateState == TemplateState.Abyss))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
                else
                {
                    return PathFinderPath.Null;
                }

                Direction GetFirstDirection()
                {
                    if (arc.checkDirectionState(Direction.D, TemplateState.Floor)) return Direction.U;
                    if (arc.checkDirectionState(Direction.U, TemplateState.Floor)) return Direction.D;
                    if (arc.checkDirectionState(Direction.R, TemplateState.Floor)) return Direction.L;
                    if (arc.checkDirectionState(Direction.L, TemplateState.Floor)) return Direction.R;
                    return Direction.Null;
                }
            }
        }
    }
}