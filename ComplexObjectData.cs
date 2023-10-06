using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComplexObjectData : DeepClonable
{
    public readonly ComplexObjectName ComplexObjectName;

    public bool alive;
    public bool hasAI;
    public bool addRandomEquip;
    public int equipLevelBonus;

    [SerializeField] public ComponentReferece<CreatureComponent> creatureComponent;
    [SerializeField] public ComponentReferece<MoraleComponent> moraleComponent;
    [SerializeField] public ComponentReferece<SquadMemberComponent> squadMemberComponent;
    [SerializeField] public ComponentReferece<EquipmentComponent> equipmentComponent;
    [SerializeField] public List<EnemyTagBufferElement> enemyTagElements = new List<EnemyTagBufferElement>();
    [SerializeField] public List<AbilityElement> availableAbilities = new List<AbilityElement>();
    [SerializeField] public List<MissingBodypartBufferElement> missingBodyparts = new List<MissingBodypartBufferElement>();
    [SerializeField] public List<PerkElement> perks = new List<PerkElement>();

    [SerializeField] public SimpleObjectData Body;
    [SerializeField] public SimpleObjectData Head;
    [SerializeField] public SimpleObjectData LowerBody;
    [SerializeField] public SimpleObjectData RightFrontLeg;
    [SerializeField] public SimpleObjectData LeftFrontLeg;
    [SerializeField] public SimpleObjectData RightRearLeg;
    [SerializeField] public SimpleObjectData LeftRearLeg;
    [SerializeField] public SimpleObjectData RightArm;
    [SerializeField] public SimpleObjectData LeftArm;
    [SerializeField] public SimpleObjectData RightClaw;
    [SerializeField] public SimpleObjectData LeftClaw;
    [SerializeField] public SimpleObjectData Tentacle0;
    [SerializeField] public SimpleObjectData Tentacle1;
    [SerializeField] public SimpleObjectData Tentacle2;
    [SerializeField] public SimpleObjectData Tentacle3;
    [SerializeField] public SimpleObjectData Tentacle4;
    [SerializeField] public SimpleObjectData Tail;

    [SerializeField] public SimpleObjectData itemInMainHand;
    [SerializeField] public SimpleObjectData itemInOffHand;
    [SerializeField] public SimpleObjectData itemOnHead;
    [SerializeField] public SimpleObjectData itemOnChest;

    [SerializeField] public List<SimpleObjectData> itemsInInventory = new List<SimpleObjectData>();

    public ComplexObjectData(ComplexObjectName complexObjectName)
    {
        ComplexObjectName = complexObjectName;
    }

    public SimpleObjectData[] BodyParts
    {
        get
        {
            return new SimpleObjectData[]
            {
                Head,
                LowerBody,
                RightFrontLeg,
                LeftFrontLeg,
                RightRearLeg,
                LeftRearLeg,
                RightArm,
                LeftArm,
                RightClaw,
                LeftClaw,
                Tentacle0,
                Tentacle1,
                Tentacle2,
                Tentacle3,
                Tentacle4,
                Tail,
            };
        }
    }
}