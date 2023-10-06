using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class PressurePlatesSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<PressurePlateComponent>());
        var array = query.ToEntityArray(Allocator.Temp);
        foreach (var entity in array)
        {
            var currentTile = entity.CurrentTile();
            var pressurePlate = entity.GetComponentData<PressurePlateComponent>();

            if (!pressurePlate.pressed)
            {
                var items = new List<Entity>() { currentTile.SolidLayer };
                items.AddRange(currentTile.DropLayer);

                foreach (var item in items)
                {
                    if (item != Entity.Null && !item.HasComponent<ImpulseComponent>() && !item.HasComponent<PlateIgnoringTag>())
                    {
                        Debug.Log("Plate Clicks!");
                        PopUpCreator.CreatePopUp(entity.CurrentTile().position, "Click!");
                        SoundSystem.ScheduleSound(SoundName.PressurePlate, entity.CurrentTile());

                        pressurePlate.pressed = true;
                    }
                }
                entity.SetComponentData(pressurePlate);
                if (pressurePlate.pressed)
                {
                    var tiles = BaseMethodsClass.GetTilesInRadius(currentTile, 10).ToList();
                    tiles.Shuffle();
                    ;
                    var bestTrapTile = TileData.Null;
                    foreach (var trapTile in tiles)
                    {
                        if (trapTile.SolidLayer != Entity.Null
                        && trapTile != currentTile
                        && !trapTile.GetNeibors(true).Contains(currentTile)
                        && trapTile.visionBlocked
                        && trapTile.ClearLineOfSight(currentTile))
                        {
                            bestTrapTile = trapTile;
                            break;
                        }
                    }
                    var targetTiles = new List<TileData>() { currentTile };

                    //for (int i = 0; i < 2; i++)
                    //{
                    //    var targetTile = currentTile.GetNeibors(true).RandomItem();
                    //    if (targetTile.ClearLineOfSight(trapTile))
                    //    {
                    //        targetTiles.Add(targetTile);
                    //    }
                    //}
                    /////////////////////////////
                    ///

                    if (bestTrapTile != TileData.Null)
                    {
                        foreach (var targetTile in targetTiles)
                        {
                            SpawnSystem.ScheduleSpawn(SimpleObjectName.Arrow, bestTrapTile);
                            SoundSystem.ScheduleSound(SoundName.Woosh, bestTrapTile);
                            DelayedActionsUpdaterOnCycle.ScheduleAction(delegate
                            {
                                var projectile = bestTrapTile.DropLayer.First(e => e.GetComponentData<SimpleObjectNameComponent>().simpleObjectName == SimpleObjectName.Arrow);
                                projectile.AddComponentData(new ImpulseComponent(targetTile - bestTrapTile, 70, UnityEngine.Random.Range(5, 10), entity));
                            });
                        }
                    }
                    currentTile.hasActivePressurePlate = false;
                    currentTile.Save();
                }
            }
        }
        array.Dispose();
        UpdateECB();
    }
}

public struct PressurePlateComponent : IComponentData
{
    public bool pressed;
}