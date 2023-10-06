using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct EvaluationInputData : IComponentData
{
    [ReadOnly] public int movementCost;
    [ReadOnly] public float morale;
    [ReadOnly] public int viewDistance;

    [ReadOnly] public bool isPlayerSquadmate;
    [ReadOnly] public bool hasRangedWeapon;
    [ReadOnly] public bool hasAmmo;
    [ReadOnly] public bool rangedWeaponLoaded;
    [ReadOnly] public bool hasClearTraectory;
    [ReadOnly] public bool canThrow;
    [ReadOnly] public bool hasPolearm;
    [ReadOnly] public bool isTrader;

    [ReadOnly] public TileData playerTile;
    [ReadOnly] public TileData squadLeaderTile;
    [ReadOnly] public TileData targetTile;

    [ReadOnly] public WalkabilityDataComponent walkabilityData;

    [ReadOnly] public Entity self;
    [ReadOnly] public Entity target;
    [ReadOnly] public Entity ItemInMainHand;
    [ReadOnly] public TileData currentTile;
    [ReadOnly] public TileData homeTile;
    [ReadOnly] public OrderComponent orderComponent;

    public Unity.Mathematics.Random random;

}
public static class Behaviours
{
    public interface IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName { get; }
        public abstract float Evaluate(EvaluationInputData data);

        public abstract AbilityComponent GetAbility(EvaluationInputData data, PathComponent path);

        public abstract PathComponent GetFindPathComponent(EvaluationInputData data);
    }


    [BurstCompile]
    public struct WaitBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Wait;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            return 0;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            AbilityComponent component = new AbilityComponent() { ability = AbilityName.SpendTime10 };

            return component;
        }
    }

    public struct RoamBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Roam;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (KatabasisUtillsClass.ChanceThreadSafe(data.random, 300f / data.movementCost))

            {
                var neibors = data.currentTile.GetNeiborsSafe(true);

                foreach (var tile in neibors)
                {
                    if (tile.isWalkableBurstSafe(data.walkabilityData, data.currentTile.position))
                    {
                        return 0.1f;
                    }
                }
            }

            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            var component = new AbilityComponent();

            foreach (var tile in data.currentTile.GetNeiborsSafe(true).Shuffle(data.random))
            {
                if (tile.isWalkableBurstSafe(data.walkabilityData, data.currentTile.position))
                {
                    component = new AbilityComponent(AbilityName.MakeStep, tile);
                    break;
                }
            }

            return component;
        }
    }

    public struct StepAsideBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.StepAside;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null && KatabasisUtillsClass.ChanceThreadSafe(data.random, 1000f / data.movementCost))
            {
                if (data.currentTile.IsInRangeOfOne(data.targetTile))
                {
                    foreach (var item in data.currentTile.GetNeiborsSafe(true))
                    {
                        if (item.IsInRangeOfOne(data.targetTile))
                        {
                            if (KatabasisUtillsClass.ChanceThreadSafe(data.random, 10)) return 1.1f;
                            return 0.5f;
                        }
                    }
                }
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            var component = new AbilityComponent();

            foreach (var item in data.currentTile.GetNeiborsSafe(true).Shuffle(data.random))
            {
                if (item.IsInRangeOfOne(data.targetTile))
                {
                    component = new AbilityComponent(AbilityName.MakeStep, item);
                    break;
                }
            }

            return component;
        }
    }

    public struct StepBackBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.StepBack;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null
                && data.morale < 50
                && KatabasisUtillsClass.ChanceThreadSafe(data.random, 1000f / data.movementCost))
            {
                if (data.currentTile.IsInRangeOfOne(data.targetTile))
                {
                    foreach (var item in data.currentTile.GetNeiborsSafe(true))
                    {
                        if (!item.IsInRangeOfOne(data.targetTile))
                        {
                            if (KatabasisUtillsClass.ChanceThreadSafe(data.random, 10)) return 1.1f;
                            return 0.5f;
                        }
                    }
                }
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            var component = new AbilityComponent();

            foreach (var item in data.currentTile.GetNeiborsSafe(true).Shuffle(data.random))
            {
                if (!item.IsInRangeOfOne(data.targetTile))
                {
                    component = new AbilityComponent(AbilityName.MakeStep, item);
                    break;
                }
            }

            return component;
        }
    }

    public struct AttackBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Atack;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null)
            {
                if (data.currentTile.IsInRangeOfOne(data.targetTile) || (data.currentTile.IsInRangeOfTwo(data.targetTile) && data.hasPolearm))
                {
                    return 1;
                }
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            AbilityComponent component = new AbilityComponent() { ability = AbilityName.Attack, targetTile = data.targetTile };
            return component;
        }
    }

    public struct FleeBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Flee;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null && data.morale < AiSystem.moraleFleeTreshold)
            {
                foreach (var tile in data.currentTile.GetNeiborsSafe(true))
                {
                    if (tile.isWalkableBurstSafe(data.walkabilityData, data.currentTile.position))
                    {
                        if (!tile.IsInRangeOfOne(data.targetTile))
                        {
                            return 2;
                        }
                    }
                }
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            AbilityComponent component = new AbilityComponent() { ability = AbilityName.Attack, targetTile = data.targetTile };
            foreach (var tile in data.currentTile.GetNeiborsSafe(true).Shuffle(data.random))
            {
                if (tile.isWalkableBurstSafe(data.walkabilityData, data.currentTile.position))
                {
                    if (!tile.IsInRangeOfOne(data.targetTile))
                    {
                        component = new AbilityComponent(AbilityName.MakeStep, tile);
                    }
                }
            }

            return component;
        }
    }

    public struct ChaseBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Chase;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(data.currentTile.position, data.targetTile.position);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null)
            {
                return 0.5f;
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            var component = new AbilityComponent(AbilityName.SpendTime10);

            if (path.found)
            {
                component = new AbilityComponent() { ability = AbilityName.MakeStep, targetTile = path.CurrentPathPosition.ToTileData() };

                //if (data.HasAbility(Ability.Dig))
                //{
                //    if (path.nodes[0].SolidLayer != Entity.Null && !path.nodes[0].ToMapIndex() SolidLayer.HasComponent<AIComponent>())
                //    {
                //        component = new AbilityComponent() { ability = Ability.Dig, targetTile = path.nodes[0] };

                //    }
                //}
            }
            else
            {
                var roamBehaviour = new RoamBehaviourStruct();
                var roamEvaluation = roamBehaviour.Evaluate(data);
                if (roamEvaluation > 0)
                {
                    component = roamBehaviour.GetAbility(data, path);
                }
            }

            return component;
        }
    }

    public struct ShootBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Shoot;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null)
            {
                if (data.hasRangedWeapon && data.rangedWeaponLoaded && data.hasAmmo)
                {
                    if (data.hasClearTraectory)
                    {
                        return 2;
                    }
                }
            }

            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            if (data.targetTile == TileData.Null) throw new Exception("target tile is null");

            Debug.Log("target tile is " + data.targetTile);
            AbilityComponent component = new AbilityComponent(AbilityName.Shoot, data.targetTile);
            return component;
        }
    }

    public struct ThrowBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Throw;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null)
            {
                if (data.canThrow)
                {
                    if (data.hasClearTraectory)
                    {
                        return 2;
                    }
                }
            }

            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            AbilityComponent component = new AbilityComponent(AbilityName.Throw, data.targetTile, data.ItemInMainHand, Power: math.max(15, data.viewDistance));
            return component;
        }
    }

    public struct ReloadBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Reload;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.hasRangedWeapon)
            {
                if (!data.rangedWeaponLoaded && data.hasAmmo)
                {
                    return 2;
                }
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            AbilityComponent component = new AbilityComponent(AbilityName.ReloadWeapon);
            return component;
        }
    }

    public struct PicupItemBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Picup;

        public float Evaluate(EvaluationInputData data)
        {
            if (data.target != Entity.Null)
            {
                return 0;
            }
            else
            {
                return 0;
            }
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            var abilityComponent = new AbilityComponent(AbilityName.ReplaceItemInMainHand, targetEntity: data.target);
            return abilityComponent;
        }

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(true);
        }
    }

    public struct FollowLeaderBehaviourStruct : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.Follow;

        public float Evaluate(EvaluationInputData data)
        {
            if (data.squadLeaderTile != data.currentTile)
            {
                if (data.currentTile.GetSqrDistance(data.squadLeaderTile) > 9)
                {
                    if (data.isPlayerSquadmate)
                    {
                        return 0.6f;
                    }
                    return 0.5f;
                }
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            var component = new AbilityComponent(AbilityName.SpendTime10);

            if (path.found)
            {
                component = new AbilityComponent() { ability = AbilityName.MakeStep, targetTile = path.CurrentPathPosition.ToTileData() };
            }
            return component;
        }

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(data.currentTile.position, data.squadLeaderTile.position);
        }
    }

    public struct ReturnToHomePointBehaviourStruct : IAIBehaviourStruct
    {

        public AiBehaviourName behaviourName => AiBehaviourName.ReturnHome;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(data.currentTile.position, data.homeTile.position);
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.squadLeaderTile == data.currentTile && !data.homeTile.position.Equals(int2.zero) && data.homeTile.GetSqrDistance(data.currentTile) > 16)
            {
                return 0.2f;
            }
            return -1;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            var component = new AbilityComponent(AbilityName.SpendTime10);

            if (path.found)
            {
                // Debug.Log("!");
                component = new AbilityComponent() { ability = AbilityName.MakeStep, targetTile = path.CurrentPathPosition.ToTileData() };

                //if (data.HasAbility(Ability.Dig))
                //{
                //    if (path.nodes[0].SolidLayer != Entity.Null && !path.nodes[0].SolidLayer.HasComponent<AIComponent>())
                //    {
                //        component = new AbilityComponent() { ability = Ability.Dig, targetTile = path.nodes[0] };

                //    }
                //}
            }
            else
            {
                var roamBehaviour = new RoamBehaviourStruct();
                var roamEvaluation = roamBehaviour.Evaluate(data);
                if (roamEvaluation > 0)
                {
                    component = roamBehaviour.GetAbility(data, path);
                }
            }

            return component;
        }
    }

    public struct GoToOrder : IAIBehaviourStruct
    {
        public AiBehaviourName behaviourName => AiBehaviourName.GoToOrder;

        public PathComponent GetFindPathComponent(EvaluationInputData data)
        {
            return new PathComponent(data.currentTile.position, data.orderComponent.targetTileIndex.ToMapPosition());
        }

        public float Evaluate(EvaluationInputData data)
        {
            if (data.orderComponent.orderType == OrderType.GoTo && data.orderComponent.targetTileIndex != data.currentTile.index)
            {
                return 3;
            }
            else return 0;
        }

        public AbilityComponent GetAbility(EvaluationInputData data, PathComponent path)
        {
            return new ChaseBehaviourStruct().GetAbility(data, path);
        }
    }
}
