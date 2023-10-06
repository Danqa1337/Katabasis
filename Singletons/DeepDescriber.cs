using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class DeepDescriber : MonoBehaviour
{
    public Label Description;
    public Label NameLabel;
    public RawImage portrait;
    public CircularList<Entity> Objects;
    public Button leftButton, rightButton;
    public RenderTexture renderTexture;
    private static LocalizationVariant currentLocaliztion => LocalizationManager.CurrentLocaliztion;

    private void Awake()
    {
        leftButton.onClick.AddListener(Previous);
        rightButton.onClick.AddListener(Next);
    }

    private void OnEnable()
    {
        UiManager.OnShowCanvas += DoOnCanvasShow;
    }

    private void OnDisable()
    {
        UiManager.OnShowCanvas -= DoOnCanvasShow;
    }

    private void DoOnCanvasShow(UIName uIName)
    {
        if (uIName == UIName.Describer)
        {
            DescribeTile(Selector.SelectedTile);
        }
    }

    public void DescribeTile(TileData tile)
    {
        if (tile != TileData.Null)
        {
            Objects = new CircularList<Entity>();

            if (tile.SolidLayer != Entity.Null && !tile.SolidLayer.IsInInvis())
            {
                Objects.Add(tile.SolidLayer);
            }

            foreach (var item in tile.DropLayer)
            {
                /*if (!item.IsInInvis)*/
                Objects.Add(item);
            }

            if (tile.LiquidLayer != Entity.Null) Objects.Add(tile.LiquidLayer);

            foreach (var item in tile.GroundCoverLayer)
            {
                /*if (!item.IsInInvis && item != tile.FloorLayer[0])*/
                Objects.Add(item);
            }

            if (tile.FloorLayer != Entity.Null) Objects.Add(tile.FloorLayer);

            leftButton.interactable = Objects.Count > 1;
            rightButton.interactable = Objects.Count > 1;
            if (Objects.Count > 0)
            {
                DescribeEntity(Objects[0]);
            }
            else
            {
                DescribeAbyss();
            }
        }
        void DescribeAbyss()
        {
            var abyss = SimpleObjectsDatabase.GetObjectData(SimpleObjectName.AbyssSide);
            //NameLabel.SetText(abyss.staticData.realName);
            // Description.SetText(abyss.staticData.description);
        }
    }

    public void DescribeEntity(Entity entity)
    {
        if (entity == null) throw new System.ArgumentNullException("trying to describe null");

        PhotoCamera.MakeFullPhoto(entity, renderTexture);
        NameLabel.SetText(DescriberUtility.GetName(entity));

        Cases cases = new Cases(entity);
        string descriptionText = DescriberUtility.GetDescription(entity);
        var objecType = entity.GetComponentData<ObjectTypeComponent>().objectType;

        switch (objecType)
        {
            case ObjectType.Solid:
                if (entity.HasComponent<AIComponent>())
                {
                    descriptionText += BehaviourParametersDescription();
                    descriptionText += EquipParametersDescription();
                }
                descriptionText += PhysicalParametersDescription();
                if (entity.HasComponent<AnatomyComponent>()) descriptionText += AnatomyParametersDescription();
                descriptionText += EffectsDescription();

                break;

            case ObjectType.Drop:
                goto case ObjectType.Solid;

            case ObjectType.Floor:
                break;

            case ObjectType.Liquid:
                break;

            case ObjectType.GroundCover:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        Description.SetText(descriptionText);

        string PhysicalParametersDescription()
        {
            var description = "";

            var physics = entity.GetComponentData<PhysicsComponent>();
            switch (currentLocaliztion)
            {
                case LocalizationVariant.English:

                    description += "\n cases.nominative" + " is " + physics.size.ToString().ToLower();

                    break;

                case LocalizationVariant.Russian:
                    break;

                default:
                    break;
            }

            //if (thing.flammabilityTreshold < 300) _string += "\n" + cases.nominative + " can burn well";

            return description;
        }
        string AnatomyParametersDescription()
        {
            var description = "";
            var anatomy = entity.GetComponentData<AnatomyComponent>();
            var damagedParts = anatomy.GetBodyPartsNotNull().Where(p => p.GetComponentData<DurabilityComponent>().GetDurabilityPercent != 100).ToList();
            switch (currentLocaliztion)
            {
                case LocalizationVariant.English:

                    if (entity.HasComponent<CreatureComponent>())
                    {
                        var creature = entity.GetComponentData<CreatureComponent>();
                        var healthPercent = 100;

                        for (int i = 25; i <= 100; i += 25)
                        {
                            if (healthPercent <= i)
                            {
                                if (i == 25) description += "\n" + cases.nominative + " looks deadly wounded";
                                if (i == 50) description += "\n" + cases.nominative + " looks severely wounded ";
                                if (i == 75) description += "\n" + cases.nominative + " looks wounded ";
                                if (i == 100) description += "\n" + cases.nominative + " looks healthy";
                                break;
                            }
                        }
                    }

                    if (damagedParts.Count > 1)
                    {
                        foreach (var part in damagedParts)
                        {
                            description += "\n " + cases.genitive + " " + part.GetComponentData<BodyPartComponent>().bodyPartTag + " "
                                + getDurabilityDescription(part);
                        }
                    }
                    else
                    {
                        description += "\n " + cases.nominative + " " + getDurabilityDescription(entity);
                    }
                    break;

                case LocalizationVariant.Russian:

                    if (damagedParts.Count > 1)
                    {
                        description += "\n Некоторые части тела этого персонажа повреждены";
                        foreach (var part in damagedParts)
                        {
                            var partName = LocalizationManager.GetString("Bodyparts", part.GetComponentData<BodyPartComponent>().bodyPartTag.ToString());
                            description += "\n " + partName + " - "
                                + getDurabilityDescription(part);
                        }
                    }

                    break;

                default:
                    break;
            }
            var missingParts = entity.GetBuffer<MissingBodypartBufferElement>();
            if (missingParts.Length > 0)
            {
                switch (currentLocaliztion)
                {
                    case LocalizationVariant.English:

                        foreach (var limb in missingParts)
                        {
                            description += "\n " + cases.genitive + " " + limb.tag.ToString() + " is missing.";
                        }
                        break;

                    case LocalizationVariant.Russian:

                        description += "\nНекоторые части тела полностью отсутсвуют: ";
                        foreach (var part in missingParts)
                        {
                            description += "\n " + LocalizationManager.GetString("Bodyparts", part.tag.ToString());
                        }

                        break;

                    default:
                        break;
                }
            }

            return description;
        }
        string getDurabilityDescription(Entity entity)
        {
            var durability = entity.GetComponentData<DurabilityComponent>();
            var durPercent = durability.GetDurabilityPercent;
            string durabilityDescription = "";

            for (int i = 25; i <= 100; i += 25)
            {
                if (durPercent <= i)
                {
                    switch (currentLocaliztion)
                    {
                        case LocalizationVariant.English:
                            break;
                            if (i == 25) durabilityDescription = " looks trashed";
                            if (i == 50) durabilityDescription = " looks severely damaged ";
                            if (i == 75) durabilityDescription = " looks damaged ";
                            if (i == 100) durabilityDescription = " looks intact";
                        case LocalizationVariant.Russian:
                            if (i == 25) durabilityDescription = "раздолбано";
                            if (i == 50) durabilityDescription = "сильно повреждено";
                            if (i == 75) durabilityDescription = "повреждено";
                            if (i == 100) durabilityDescription = "целое";
                            break;

                        default:
                            break;
                    }

                    break;
                }
            }
            return durabilityDescription;
        }

        string EquipParametersDescription()
        {
            var equip = entity.GetComponentData<EquipmentComponent>();
            var anatomy = entity.GetComponentData<AnatomyComponent>();
            string _string = "";

            switch (currentLocaliztion)
            {
                case LocalizationVariant.English:
                    if (equip.itemInMainHand != Entity.Null) _string += "\n" + cases.nominative + " weilds a " + equip.itemInMainHand.GetName();
                    if (equip.itemOnChest != Entity.Null) _string += "\n" + cases.nominative + " wears " + equip.itemOnChest.GetName() + " on " + cases.genitive + "chest.";
                    if (equip.itemOnHead != Entity.Null) _string += "\n" + cases.nominative + " wears " + equip.itemOnHead.GetName() + " on " + cases.genitive + " head.";
                    break;

                case LocalizationVariant.Russian:
                    var allequipment = equip.GetEquipmentNotNull();
                    if (allequipment.Count > 0)

                        _string += "\nЭтот персонаж экипирован следующими предметами: ";
                    foreach (var item in allequipment)
                    {
                        _string += "\n " + DescriberUtility.GetName(item);
                    }

                    break;

                default:
                    break;
            }

            return _string;
        }
        string BehaviourParametersDescription()
        {
            var creature = entity.GetComponentData<CreatureComponent>();
            var ai = entity.GetComponentData<AIComponent>();
            string _string = "";
            if (entity.HasComponent<AIComponent>())
            {
                switch (currentLocaliztion)
                {
                    case LocalizationVariant.English:

                        if (entity.IsHostile(Player.PlayerEntity)) _string += "\n" + cases.nominative + " is agressive agains you";
                        float speedDiference = StatsCalculator.CalculateMovementCost(entity, entity.CurrentTile()) - StatsCalculator.CalculateMovementCost(Player.PlayerEntity, Player.PlayerEntity.CurrentTile());
                        if (speedDiference < 0) _string += "\n" + cases.nominative + " can move faster then you.";
                        if (speedDiference == 0) _string += "\n" + cases.nominative + " can move as fast as you.";
                        if (speedDiference > 0) _string += "\n" + "You move faster then " + cases.accusative + ".";

                        break;

                    case LocalizationVariant.Russian:

                        if (entity.IsHostile(Player.PlayerEntity))
                        {
                            _string += "\nЭтот персонаж враждебен по отношению к вам.";
                        }
                        else
                        {
                            if (Registers.SquadsRegister.AreSquadmates(Player.PlayerEntity, entity))
                            {
                                _string += "\nЭтот персонаж - член вашего отряда.";
                            }
                            else
                            {
                                _string += "\nЭтот персонаж не обращает на вас внимания.";
                            }
                        }

                        break;

                    default:
                        break;
                }
            }

            return _string;
        }
        string EffectsDescription()
        {
            string _string = "";
            var bodyparts = entity.HasComponent<AnatomyComponent>() ? entity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull() : new List<Entity>() { entity };
            var effects = new List<EffectElement>();
            foreach (var item in bodyparts)
            {
                effects.AddRange(item.GetEffects());
            }
            if (effects.Count > 0)
            {
                switch (currentLocaliztion)
                {
                    case LocalizationVariant.English:
                        _string += "\n" + cases.nominative + " is affected by following effects: ";
                        foreach (var item in effects)
                        {
                            _string += "\n " + LocalizationManager.GetString("Effects", item.EffectName.ToString());
                        }

                        break;

                    case LocalizationVariant.Russian:
                        _string += "\nНа этот объект воздействуют следующие эффекты: ";
                        foreach (var item in effects)
                        {
                            _string += "\n " + LocalizationManager.GetString("Effects", item.EffectName.ToString());
                        }
                        break;

                    default:
                        break;
                }
            }

            return _string;
        }
    }

    public void Next()
    {
        Debug.Log("next");
        DescribeEntity(Objects.Next());
    }

    public void Previous()
    {
        DescribeEntity(Objects.Prev());
    }

    private class Cases
    {
        public string nominative = "It";
        public string genitive = "Tts";
        public string accusative = "it";

        public Cases(Entity entity)
        {
            if (entity.HasComponent<CreatureComponent>())
            {
                var creature = entity.GetComponentData<CreatureComponent>();
                if (creature.gender == Gender.Male)
                {
                    nominative = "He";
                    genitive = "His";
                    accusative = "Him";
                }
                if (creature.gender == Gender.Female)
                {
                    nominative = "She";
                    genitive = "Her";
                    accusative = "Her";
                }
            }
        }
    }
}