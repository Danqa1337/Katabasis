using System;
using Unity.Entities;
using UnityEngine;

public static class StatsCalculator
{
    public const int MaxAttackCost = 50;

    private static float ScalingToFloat(Scaling scaling) => (scaling) switch
    {
        Scaling.A => 0.3f,
        Scaling.B => 0.2f,
        Scaling.C => 0.1f,
        Scaling.D => 0.05f,
        Scaling._ => 0,
    };

    private static Scaling FloatToScaling(float scaling)
    {
        if (scaling > 4) return Scaling.A;
        if (scaling > 3) return Scaling.B;
        if (scaling > 2) return Scaling.C;
        if (scaling > 1) return Scaling.D;
        return Scaling._;
    }

    public static float CalculateDPT(Entity creature, Entity weapon)
    {
        return CalculateDPT(creature.GetComponentData<CreatureComponent>(), weapon.GetComponentData<PhysicsComponent>());
    }

    public static float CalculateScalingBonus(Entity creature, Entity weapon)
    {
        return CalculateScalingBonus(creature.GetComponentData<CreatureComponent>(), weapon.GetComponentData<PhysicsComponent>());
    }

    public static int CalculateBonusDamage(Entity creature, Entity weapon)
    {
        var damage = CalculateBonusDamage(creature.GetComponentData<CreatureComponent>(), weapon.GetComponentData<PhysicsComponent>());
        if (creature.GetComponentData<AnatomyComponent>().GetBodyParts().Contains(weapon)) damage += creature.GetComponentData<CreatureComponent>().unarmedAttackDamageBonus;

        return Mathf.Max(damage, 0);
    }

    public static float CalculateTOH(Entity creature, Entity weapon)
    {
        return CalculateTOH(creature.GetComponentData<CreatureComponent>(), weapon.GetComponentData<PhysicsComponent>());
    }

    public static int CalculateAttackCost(Entity creature, Entity weapon)
    {
        var cost = CalculateAttackCost(creature.GetComponentData<CreatureComponent>(), weapon.GetComponentData<PhysicsComponent>());
        if (creature.GetComponentData<AnatomyComponent>().GetBodyParts().Contains(weapon)) cost = creature.GetComponentData<CreatureComponent>().unarmedAttacCost;
        return Math.Max(cost, 1);
    }

    public static float CalculateDPT(CreatureComponent creature, PhysicsComponent ph)
    {
        float DPA = ph.damage + CalculateBonusDamage(creature, ph);

        float THC = CalculateTOH(creature, ph) / 100;
        float ATC = CalculateAttackCost(creature, ph);

        return (DPA * THC) / ATC;
    }

    public static float CalculateScalingBonus(CreatureComponent stats, PhysicsComponent ph)
    {
        return stats.str * ScalingToFloat(ph.ScalingAGL) + stats.agl * ScalingToFloat(ph.ScalingAGL);
    }

    public static int CalculateBonusDamage(CreatureComponent stats, PhysicsComponent ph)
    {
        int bonusDamage = Mathf.FloorToInt((stats.str * ScalingToFloat(ph.ScalingSTR) + stats.agl * ScalingToFloat(ph.ScalingAGL)) / 10 * ph.damage);
        return bonusDamage;
    }

    public static float CalculateTOH(CreatureComponent stats, PhysicsComponent ph)
    {
        return ph.accuracy;
    }

    public static int CalculateAttackCost(CreatureComponent stats, PhysicsComponent ph)
    {
        var cost = (ph.baseAtackCost);

        return Mathf.Clamp(cost, 1, MaxAttackCost);
    }

    public static int CalculateAimedAttackCost(Entity stats, Entity weapon, Entity targetBodypart)
    {
        var baseAtackCost = CalculateAttackCost(stats, weapon);
        var bodypartTag = targetBodypart.HasComponent<BodyPartComponent>() ? targetBodypart.GetComponentData<BodyPartComponent>().bodyPartTag : BodyPartTag.Body;
        var hitChanceBonusFromType = GetHitChanceMultiplerFromBodypartType(bodypartTag);
        var hitChanceBonusFromSize = GetHitChanceMultiplerFromSize(targetBodypart.GetComponentData<PhysicsComponent>().size);
        var cost = baseAtackCost * 1f / hitChanceBonusFromType * 1 / hitChanceBonusFromSize;
        return Mathf.Clamp((int)cost, baseAtackCost, MaxAttackCost);
    }

    public static float GetHitChanceMultiplerFromBodypartType(BodyPartTag bodyPartTag) => bodyPartTag switch
    {
        BodyPartTag.Null => 0,
        BodyPartTag.Head => 0.5f,
        BodyPartTag.RightArm => 0.5f,
        BodyPartTag.LeftArm => 0.5f,
        BodyPartTag.RightFrontLeg => 0.5f,
        BodyPartTag.RightRearLeg => 0.5f,
        BodyPartTag.LeftFrontLeg => 0.5f,
        BodyPartTag.LeftRearLeg => 0.5f,
        BodyPartTag.RightFrontPaw => 0.5f,
        BodyPartTag.RightRearPaw => 0.5f,
        BodyPartTag.LeftFrontPaw => 0.5f,
        BodyPartTag.LeftRearPaw => 0.5f,
        BodyPartTag.Body => 1,
        BodyPartTag.Tail => 0.5f,
        BodyPartTag.Tentacle => 0.5f,
        BodyPartTag.FirstTentacle => 0.5f,
        BodyPartTag.SecondTentacle => 0.5f,
        BodyPartTag.Teeth => 0,
        BodyPartTag.RightClaw => 0.5f,
        BodyPartTag.LeftClaw => 0.5f,
        BodyPartTag.Fists => 0,
        BodyPartTag.LowerBody => 0.5f,
        _ => throw new ArgumentOutOfRangeException(nameof(bodyPartTag), bodyPartTag, null)
    };

    public static float GetHitChanceMultiplerFromSize(Size size) => size switch
    {
        Size.Tiny => 1,
        Size.Small => 2,
        Size.Medium => 3,
        Size.Large => 4,
        Size.Huge => 5,
        _ => 3,
    };
    
    public static int CalculateMovementCost(Entity creature, TileData tile)
    {
        if (creature.HasComponent<CreatureComponent>())
        {
            float baseMovementCost = creature.GetComponentData<CreatureComponent>().baseMovementCost;
            if (tile.LiquidLayer != Entity.Null)
            {
                if (creature.HasComponent<AmphibiousTag>()) baseMovementCost /= 3f;
                else
                {
                    baseMovementCost *= 1.5f;
                }
                if (creature.HasComponent<FleingTag>())
                {
                    baseMovementCost *= 0.5f;
                }
            }

            return (int)baseMovementCost;
        }
        throw new Exception(creature.GetName() + " is not creature!");
    }
    public static int CalculateViewDistance(Entity creature, Location location)
    {
        if (creature.HasComponent<CreatureComponent>())
        {
            var baseViewDistance = creature.GetComponentData<CreatureComponent>().baseViewDistance;
            var multipler = 1f;
            if (creature.IsPlayer() && location.GenerationPreset == GenerationPresetName.Pit)
            {
                multipler = 0.25f;
            } 
            return (int)(baseViewDistance * multipler);
        }
        throw new Exception(creature.GetName() + " is not creature!");
    }
    public static int CalculateViewDistance(CreatureComponent creatureComponent, Location location)
    {
        var baseViewDistance = creatureComponent.baseViewDistance;
        var multipler = 1f;
        if (location.GenerationPreset == GenerationPresetName.Pit)
        {
            multipler = 0.25f;
        } 
        return (int)(baseViewDistance * multipler);
       
    }
}