using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum TempObjectType
{
    SmallDust,
    LargeDust,
    CuttingAttack,
    PiercingAttack,
    Explosion,
    InvisParticels,
    WaterRipple,
    SacrificialFlame,
    TrailParticles,
}

[DisableAutoCreation]
public partial class TempObjectSystem : MySystemBase
{
    public static HashSet<SpawnTempObjectComponent> tempObjectList = new HashSet<SpawnTempObjectComponent>();

    protected override void OnUpdate()
    {
        foreach (var tempComponent in tempObjectList)
        {
            PoolableItem tempGameObject = null;
            Vector3 scale = new Vector3(1, 1, 1);
            Quaternion rotation = quaternion.identity;

            switch (tempComponent.tempObjectType)
            {
                case TempObjectType.LargeDust:
                    if (tempComponent.position.ToFloat2().ToTileData().LiquidLayer != Entity.Null)
                    {
                        tempGameObject = Pooler.Take("WaterRipples", Vector3.zero);
                    }
                    else
                    {
                        tempGameObject = Pooler.Take("LargeDust", Vector3.zero);
                    }

                    break;

                case TempObjectType.CuttingAttack:
                    tempGameObject = Pooler.Take("CuttingAttack", Vector3.zero);
                    calculateParametersForAtackAnimation();
                    break;

                case TempObjectType.PiercingAttack:
                    tempGameObject = Pooler.Take("PiercingAttack", Vector3.zero);
                    calculateParametersForAtackAnimation();
                    break;

                case TempObjectType.Explosion:
                    tempGameObject = Pooler.Take("Explosion", Vector3.zero);

                    break;

                case TempObjectType.InvisParticels:
                    tempGameObject = Pooler.Take("InvisParticles", Vector3.zero);
                    break;

                case TempObjectType.SmallDust:
                    tempGameObject = Pooler.Take("SmallDust", Vector3.zero);
                    break;

                case TempObjectType.WaterRipple:
                    tempGameObject = Pooler.Take("WaterRipples", Vector3.zero);
                    break;

                case TempObjectType.SacrificialFlame:
                    tempGameObject = Pooler.Take("SacrificialFlame", Vector3.zero);

                    break;

                case TempObjectType.TrailParticles:
                    tempGameObject = Pooler.Take("TrailParticles", Vector3.zero);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (tempGameObject != null)
            {
                tempGameObject.transform.position = tempComponent.position;
                tempGameObject.transform.localScale = scale;
                //tempGameObject.transform.rotation = rotation;
            }

            void calculateParametersForAtackAnimation()
            {
                rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, tempComponent.direction) + UnityEngine.Random.Range(-20, 20));
                float r = UnityEngine.Random.Range(0.9f, 1.1f);
                float w = Mathf.Clamp(2 / 2, 0.5f, 2);
                scale = new Vector3(1, 1, 1);
            }
        }
        tempObjectList.Clear();
    }

    public static void SpawnTempObject(TempObjectType type, Vector3 position)
    {
        tempObjectList.Add(new SpawnTempObjectComponent(type, position));
    }

    public static void SpawnTempObject(TempObjectType type, TileData tile)
    {
        SpawnTempObject(type, tile.position.ToRealPosition(Entity.Null));
    }
}

public struct SpawnTempObjectComponent
{
    public Vector3 position;
    public float2 direction;
    public TempObjectType tempObjectType;

    public SpawnTempObjectComponent(TempObjectType type, Vector3 position)
    {
        this.position = position;
        this.tempObjectType = type;
        this.direction = float2.zero;
    }
}