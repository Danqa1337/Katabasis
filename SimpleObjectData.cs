using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Animations;
using UnityEngine;

[Serializable]
public class SimpleObjectData : DeepClonable
{
    public readonly SimpleObjectName SimpleObjectName;
    public TemplateState defaultTileState;
    public SortingLayer sortingLayer;

    public bool altSpriteDrown;
    public int rndSpriteNum;
    public float worth;
    public ObjectTypeComponent objectTypeComponent;
    public CurrentTileComponent currentTileComponent;

    [SerializeField] public ComponentReferece<PhysicsComponent> physicsComponent;
    [SerializeField] public ComponentReferece<DecayComponent> decayComponent;
    [SerializeField] public ComponentReferece<InternalLiquidComponent> internalLiquidComponent;
    [SerializeField] public ComponentReferece<DurabilityComponent> durabilityComponent;
    [SerializeField] public ComponentReferece<ImpulseComponent> impulseComponent;
    [SerializeField] public ComponentReferece<LockComponent> lockComponent;
    [SerializeField] public ComponentReferece<DoorComponent> doorComponent;
    [SerializeField] public ComponentReferece<ObjectSoundData> objectSoundData;
    [SerializeField] public ComponentReferece<BodyPartComponent> bodyPartComponent;
    [SerializeField] public ComponentReferece<RangedWeaponComponent> rangedWeaponComponent;
    [SerializeField] public ComponentReferece<StairsComponent> stairsComponent;
    [SerializeField] public ComponentReferece<KeyComponent> keyComponent;
    [SerializeField] public ComponentReferece<EatableComponent> eatableComponent;
    [SerializeField] public ComponentReferece<UpgradeComponent> upgradeComponent;

    [SerializeField] public List<DropElement> dropElements = new List<DropElement>();
    [SerializeField] public List<EffectOnHitElement> effectsOnHit = new List<EffectOnHitElement>();
    [SerializeField] public List<EffectOnApplyingElement> effectsOnApplying = new List<EffectOnApplyingElement>();
    [SerializeField] public List<TagBufferElement> tagElements = new List<TagBufferElement>();
    [SerializeField] public List<EffectElement> activeEffects = new List<EffectElement>();

    public SimpleObjectData(SimpleObjectName simpleObjectName)
    {
        SimpleObjectName = simpleObjectName;
    }
}