using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisableAutoCreation]
public partial class ExplosionSystem : MySystemBase
{
    private static HashSet<ExplosionData> explosionDataList = new HashSet<ExplosionData>();

    protected override void OnUpdate()
    {
        foreach (var explosionData in explosionDataList)
        {
            var power = explosionData.power;
            var epicenter = explosionData.position.ToTileData();
            var affectedTiles = epicenter.GetNeibors(true).ToList();
            var shockWaveEntity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            shockWaveEntity.SetName("ShockWave");
            shockWaveEntity.AddComponentData(new PhysicsComponent() { damage = 1 });
            affectedTiles.Add(epicenter);

            TempObjectSystem.SpawnTempObject(TempObjectType.Explosion, epicenter);

            foreach (var tile in affectedTiles)
            {
                var affectedObjects = new List<Entity>();
                if (tile.SolidLayer != Entity.Null)
                {
                    affectedObjects.Add(tile.SolidLayer);
                }
                foreach (var item in tile.DropLayer)
                {
                    affectedObjects.Add(item);
                }

                for (int i = 0; i < affectedObjects.Count; i++)
                {
                    var obj = affectedObjects[i];

                    if (obj.HasComponent<AnatomyComponent>())
                    {
                        foreach (var bodyPart in obj.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
                        {
                            if (bodyPart != obj)
                            {
                                affectedObjects.Add(bodyPart);
                            }
                        }
                    }

                    if (obj.HasComponent<EquipmentComponent>())
                    {
                        foreach (var equipmentitem in obj.GetComponentData<EquipmentComponent>().GetEquipmentNotNull())
                        {
                            affectedObjects.Add(equipmentitem);
                        }
                    }
                }

                foreach (var obj in affectedObjects)
                {
                    obj.AddBufferElement(new CollisionElement((int)(power * KatabasisUtillsClass.GenerateNormalRandom(1, 0.5f)), shockWaveEntity, explosionData.responsibleEntity));
                }

                if (BaseMethodsClass.Chance(30))
                {
                    var smoke = tile.Spawn(SimpleObjectName.Smoke);
                    var cloud = smoke.GetComponentData<CloudComponent>();
                    cloud.lifeTime = UnityEngine.Random.Range(50, 100);
                    cloud.moveCoolDown = 0;
                    smoke.SetComponentData(cloud);
                }
            }

            DelayedActionsUpdaterOnCycle.ScheduleAction(delegate
            {
                var affectedTilesDelayed = epicenter.index.ToTileData().GetNeibors(true).ToList();
                affectedTilesDelayed.Add(epicenter.index.ToTileData());

                foreach (var tile in affectedTilesDelayed)
                {
                    var affectedObjects = new HashSet<Entity>();
                    var random = GetRandom();

                    if (tile.SolidLayer != Entity.Null)
                    {
                        affectedObjects.Add(tile.SolidLayer);
                    }
                    foreach (var item in tile.DropLayer)
                    {
                        affectedObjects.Add(item);
                    }

                    foreach (var obj in affectedObjects)
                    {
                        if (!obj.HasComponent<UnmovableTag>())
                        {
                            var vector = tile == epicenter ? random.NextFloat2Direction() : ((tile - epicenter) + random.NextFloat2Direction() * 0.3f).Normalize();
                            obj.AddComponentData(new ImpulseComponent(vector, (int)(power * KatabasisUtillsClass.GenerateNormalRandom(1, 0.5f)), random.NextInt(3, 10), explosionData.responsibleEntity));
                        }
                    }
                }
            });
            SoundSystem.ScheduleSound(SoundName.Explosion, epicenter);
            Debug.Log("Explosion with power of: " + power + " on " + epicenter.position);
        }
        explosionDataList.Clear();
    }

    public static void ScheduleExplosion(TileData tile, int power, Entity responsibleEntity)
    {
        explosionDataList.Add(new ExplosionData(power, tile.position, responsibleEntity));
    }

    private struct ExplosionData
    {
        public int power;
        public int2 position;
        public Entity responsibleEntity;

        public ExplosionData(int power, int2 position, Entity responsibleEntity)
        {
            this.power = power;
            this.position = position;
            this.responsibleEntity = responsibleEntity;
        }
    }
}