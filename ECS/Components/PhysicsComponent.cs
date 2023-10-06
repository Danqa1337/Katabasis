using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct PhysicsComponent : IComponentData
{
    public Size size;
    public int baseAtackCost;
    public int damage;

    public float accuracy;
    public float aerodynamicsDamageMultiplier;

    public Scaling ScalingSTR;
    public Scaling ScalingAGL;

    public EquipTag defaultEquipTag;

    public float baseResistance;
    private int baseWorth;

    public int worth
    {
        get
        {
            return baseWorth;
        }
    }

    public float resistance
    {
        get
        {
            var value = baseResistance;
            //if(self.HasEffect(EffectType.Petrification))
            //{
            //    value += 100;
            //}
            return value;
        }
    }

    public PhysicsComponent(SimpleObjectsTable.Param param)
    {
        this.size = param.size.DecodeCharSeparatedEnumsAndGetFirst<Size>();
        this.baseAtackCost = param.baseAttackCost;
        this.damage = param.baseDamage;
        this.accuracy = param.baseAccuracity;
        this.baseResistance = param.resistance;
        this.ScalingAGL = param.scalingAGL.DecodeCharSeparatedEnumsAndGetFirst<Scaling>();
        this.ScalingSTR = param.scalingSTR.DecodeCharSeparatedEnumsAndGetFirst<Scaling>();
        this.aerodynamicsDamageMultiplier = param.aerodynamicsDamageMultipler;
        this.baseWorth = (int)param.worth;
        defaultEquipTag = param.equipTag.DecodeCharSeparatedEnumsAndGetFirst<EquipTag>();
    }
}

[System.Serializable]
public struct ImpulseComponent : IComponentData
{
    [System.NonSerialized] public Entity responsibleEntity;
    public float2 axelerationVector;
    public float2 err;
    public float H;
    public int bonusDamage;

    public TileData calculateNextTile(TileData currentTile)
    {
        float2 err;
        return calculateNextTile(currentTile, out err);
    }

    public TileData calculateNextTile(TileData currentTile, out float2 error)
    {
        var step = math.normalize(axelerationVector);

        var oldPos = currentTile.position;
        var newPosfloat = oldPos + err + step;
        var newPosInt = new int2(Mathf.RoundToInt(newPosfloat.x), Mathf.RoundToInt(newPosfloat.y));
        var difference = newPosfloat - newPosInt;
        error = difference;
        return newPosInt.ToTileData();
    }

    public ImpulseComponent(float2 axelerationVector, int bonusDamage, float h, Entity responsibleEntity)
    {
        this.axelerationVector = axelerationVector;
        this.err = 0;
        this.bonusDamage = bonusDamage;
        H = h;
        this.responsibleEntity = responsibleEntity;
    }
}

[System.Serializable]
public struct RangedWeaponComponent : IComponentData
{
    public int MaxReloadingPhases;
    public int CurrentReloadPhase;
    public int Power;
    public SimpleObjectName Ammo;
    public bool Ready => CurrentReloadPhase == MaxReloadingPhases;

    public RangedWeaponComponent(SimpleObjectsTable.Param param)
    {
        MaxReloadingPhases = 3;
        CurrentReloadPhase = 0;
        Power = 1;
        Ammo = SimpleObjectName.Arrow;
    }
}

public enum MovemetType
{
    SelfPropeled,
    Forced,
    PositionChange,
}

public struct MoveComponent : IComponentData
{
    public int prevTileId;
    public int nextTileId;
    public MovemetType movemetType;

    public MoveComponent(TileData prevTile, TileData nextTile, MovemetType movemetType)
    {
        this.prevTileId = prevTile.index;
        this.nextTileId = nextTile.index;
        this.movemetType = movemetType;
    }

    public MoveComponent(int prevTile, int nextTile, MovemetType movemetType)
    {
        this.prevTileId = prevTile;
        this.nextTileId = nextTile;
        this.movemetType = movemetType;
    }
}

[Serializable]
public struct CurrentTileComponent : IComponentData
{
    public int currentTileId;

    public TileData CurrentTile => currentTileId.ToTileData();

    public CurrentTileComponent(int currentTileId)
    {
        this.currentTileId = currentTileId;
    }
}

[Serializable]
public struct ObjectTypeComponent : IComponentData
{
    public ObjectType objectType;

    public ObjectTypeComponent(ObjectType objectType)
    {
        this.objectType = objectType;
    }
}

[System.Serializable]
public struct TagBufferElement : IBufferElementData
{
    public Tag tag;

    public TagBufferElement(Tag tag)
    {
        this.tag = tag;
    }
}

[System.Serializable]
public struct EnemyTagBufferElement : IBufferElementData
{
    public Tag tag;

    public EnemyTagBufferElement(Tag tag)
    {
        this.tag = tag;
    }
}

[System.Serializable]
public struct MissingBodypartBufferElement : IBufferElementData
{
    public BodyPartTag tag;
    public SimpleObjectName ItemName;

    public MissingBodypartBufferElement(BodyPartTag tag, SimpleObjectName itemName)
    {
        this.tag = tag;
        ItemName = itemName;
    }
}

[System.Serializable]
public struct AbilityElement : IBufferElementData
{
    public readonly AbilityName ability;

    public AbilityElement(AbilityName ability)
    {
        this.ability = ability;
    }
}

[Serializable]
public struct InternalLiquidComponent : IComponentData
{
    public SimpleObjectName liquidSpaltter;

    public InternalLiquidComponent(SimpleObjectName liquidSpaltter)
    {
        this.liquidSpaltter = liquidSpaltter;
    }

    public InternalLiquidComponent(SimpleObjectsTable.Param param)
    {
        this.liquidSpaltter = param.internalLiquid.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>();
    }
}

[Serializable]
public struct AllowedEquipTagBufferElement : IBufferElementData
{
    public EquipTag equipTag;
}

public struct SimpleObjectNameComponent : IComponentData
{
    public readonly SimpleObjectName simpleObjectName;

    public SimpleObjectNameComponent(SimpleObjectName ID)
    {
        this.simpleObjectName = ID;
    }
}

public struct ComplexObjectNameComponent : IComponentData
{
    public readonly ComplexObjectName complexObjectName;

    public ComplexObjectNameComponent(ComplexObjectName complexObjectName)
    {
        this.complexObjectName = complexObjectName;
    }
}

[Serializable]
public struct StairsComponent : IComponentData
{
    public int transitionId;
}

public struct BreakObjectComponent : IComponentData
{
    public readonly Entity ResponsibleEntity;
    public readonly DurabilityChangeReason DamageType;

    public BreakObjectComponent(Entity responsibleEntity, DurabilityChangeReason breakReaseon)
    {
        ResponsibleEntity = responsibleEntity;
        DamageType = breakReaseon;
    }
}

[Serializable]
public struct EatableComponent : IComponentData
{
    public int nutrition;

    public EatableComponent(int nutrition)
    {
        this.nutrition = nutrition;
    }

    public EatableComponent(SimpleObjectsTable.Param param)
    {
        this.nutrition = (int)(param.maxDurability * 0.25f);
    }
}

public struct ContainerComponent : IComponentData
{
}

public struct IsGoingToSwapComponent : IComponentData
{
    public Entity EntityToSwapWith;

    public IsGoingToSwapComponent(Entity entityToSwapWith)
    {
        EntityToSwapWith = entityToSwapWith;
    }
}

[Serializable]
public struct DropElement : IBufferElementData
{
    public float chance;
    public SimpleObjectName itemName;

    public DropElement(SimpleObjectName itemName, float chance = 100)
    {
        this.chance = chance;
        this.itemName = itemName;
    }
}

public struct CharacterComponent : IComponentData
{
}

[Serializable]
public struct ApplyableComponent : IComponentData
{
    public Entity self;

    public ApplyableComponent(Entity self)
    {
        this.self = self;
    }

    public List<EffectName> EffectsOnApplying()
    {
        var list = new List<EffectName>();
        foreach (var item in self.GetBuffer<EffectOnApplyingElement>())
        {
            list.Add(item.effect);
        }
        return list;
    }
}

[Serializable]
public struct EffectOnApplyingElement : IBufferElementData
{
    public EffectName effect;

    public EffectOnApplyingElement(EffectName effect)
    {
        this.effect = effect;
    }
}

[Serializable]
public struct UpgradeComponent : IComponentData
{
    public int upgrade;

    public UpgradeComponent(int upgrade)
    {
        this.upgrade = upgrade;
    }
}