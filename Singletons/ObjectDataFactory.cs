using System.Collections.Generic;
using UnityEngine;

public enum ObjectDataArgument
{
    Null,
    Any,
    EnemyTags,
    RandomEquip,
    ItemInMainHand,
    ItemInOffHand,
    ItemOnHead,
    ItemOnChest,
    ItemsInInventory,
    AddTags,
    RemoveTags,
    Tags,
    XpOnDeath,
}

public static class ObjectDataFactory
{

    public static ComplexObjectData GetRandomCreature(int level, Biome biome = Biome.Any, List<Tag> tags = null, bool withRandomEquip = false, ComplexObjectName itemName = ComplexObjectName.Null)
    {
        //if (tags == null) tags = new List<Tag>() { Tag.Creature };

        //var creatureObjectData = itemName == SimpleObjectName.Null ? SimpleObjectsDatabase.GetRandomSimpleObject(level, biome, tags) : SimpleObjectsDatabase.GetObjectData(itemName);
        //if (withRandomEquip)
        //{
        //    if (creatureObjectData.staticData.allowedWeaponTags.Count > 0
        //        || creatureObjectData.staticData.allowedWeaponTags.Contains(Tag.Any))
        //    {
        //        var weaponTags = creatureObjectData.staticData.allowedWeaponTags;
        //        var itemInMainHand = (GetRandomItem(level, biome, weaponTags)).dynamicData;
        //        creatureObjectData.dynamicData.itemInMainHand = itemInMainHand;
        //        if (BaseMethodsClass.Chance(5))
        //        {
        //            var itemInOffHand = (GetRandomItem(level, biome, new List<Tag> { Tag.Shield })).dynamicData;
        //            creatureObjectData.dynamicData.itemInOffHand = itemInOffHand;
        //        }
        //    }

        //    if (BaseMethodsClass.Chance(60)
        //        && (creatureObjectData.staticData.allowedArmorTags.Contains(Tag.Headwear)
        //        || creatureObjectData.staticData.allowedArmorTags.Contains(Tag.Any)))
        //    {
        //        var itemOnHead = (GetRandomItem(level, biome, new List<Tag> { Tag.Headwear }));
        //        if (itemOnHead != null)
        //        {
        //            creatureObjectData.dynamicData.itemOnHead = itemOnHead.dynamicData;
        //        }
        //    }

        //    if ((creatureObjectData.staticData.allowedArmorTags.Contains(Tag.Chestplate)
        //        || creatureObjectData.staticData.allowedArmorTags.Contains(Tag.Any)))
        //    {
        //        if (BaseMethodsClass.Chance(60))
        //        {
        //            var itemOnChest = (GetRandomItem(level, biome, new List<Tag> { Tag.Chestplate }));
        //            if (itemOnChest != null)
        //            {
        //                creatureObjectData.dynamicData.itemOnChest = itemOnChest.dynamicData;
        //            }
        //        }
        //        else if (BaseMethodsClass.Chance(90))
        //        {
        //            creatureObjectData.dynamicData.itemOnChest = SimpleObjectsDatabase.GetObjectData(SimpleObjectName.Toga).dynamicData;
        //        }
        //    }
        //}

        return null;
    }
    public static SimpleObjectData Upgrade(SimpleObjectData data, int upgrade)
    {
        var defaultdata = SimpleObjectsDatabase.GetObjectData(data.SimpleObjectName, true);

        var physicsComponent = data.physicsComponent.Component;
        var module = Mathf.Sign(upgrade);
        var baseDamage = defaultdata.physicsComponent.Component.damage;
        var newDamage = baseDamage;
        var baseRes = defaultdata.physicsComponent.Component.baseResistance;
        var newRes = baseRes;
        

        for (int i = 0; i < Mathf.Abs(upgrade); i++)
        {
            if (data.tagElements.Contains(new TagBufferElement(Tag.Weapon)))
            {
                newDamage += (int)(baseDamage * 0.1f * module);

            }
            if (data.tagElements.Contains(new TagBufferElement(Tag.Armor)))
            {
                newRes += baseRes * 0.1f * module;
            }
        }
        data.worth = data.worth * upgrade;
        physicsComponent.baseResistance = newRes;
        physicsComponent.damage = newDamage;
        data.upgradeComponent = new ComponentReferece<UpgradeComponent>(new UpgradeComponent(upgrade));

        return data;
    }

    public static List<ComplexObjectData> GetMultipleComplexObjectDataFromString(string input)
    {
        var splitMembersStrings = input.SplitOutsideBlocks(',', '(', ')');
        var members = new List<ComplexObjectData>();

        foreach (var memberString in splitMembersStrings)
        {
            members.Add(GetComplexObjectDataFromString(memberString));
        }
        return members;
    }

    public static ComplexObjectData GetComplexObjectDataFromString(string input)
    {
        var itemNameString = "";

        if (input.GetSubstringUntillChar('(', out itemNameString))
        {
            var data = ComplexObjectsDatabase.GetObjectData(itemNameString.DecodeCharSeparatedEnumsAndGetFirst<ComplexObjectName>());

            var argumentsString = "";
            if (input.GetSubstringBetveen('(', ')', out argumentsString)) 
            {
                var argumentsWithParametersArray = argumentsString.SplitOutsideBlocks(',', '(', ')');

                foreach (var argumentWithParametersString in argumentsWithParametersArray)
                {
                    var argumentString = "";
                    var parametersString = "";
                    var argument = ObjectDataArgument.Null;
                    if (argumentWithParametersString.GetSubstringUntillChar('(', out argumentString))
                    {
                        argumentWithParametersString.GetSubstringBetveen('(', ')', out parametersString);
                        argument = argumentString.DecodeCharSeparatedEnumsAndGetFirst<ObjectDataArgument>();
                    }
                    else
                    {
                        argument = argumentWithParametersString.DecodeCharSeparatedEnumsAndGetFirst<ObjectDataArgument>();
                    }

                    ApplyArgument(data, argument, parametersString);
                }
            }
            return data;
        }
        else
        {
            return ComplexObjectsDatabase.GetObjectData(input.DecodeCharSeparatedEnumsAndGetFirst<ComplexObjectName>());
        }


        void ApplyArgument(ComplexObjectData complexObjectData, ObjectDataArgument argument, string argumentParameters)
        {
            switch (argument)
            {
                case ObjectDataArgument.Null:
                    break;

                case ObjectDataArgument.Any:
                    break;

                case ObjectDataArgument.EnemyTags:

                    complexObjectData.enemyTagElements = new List<EnemyTagBufferElement>();
                    foreach (var parameter in argumentParameters.Split(','))
                    {
                        complexObjectData.enemyTagElements.Add(new EnemyTagBufferElement(parameter.DecodeCharSeparatedEnumsAndGetFirst<Tag>()));
                    }

                    break;

                case ObjectDataArgument.RandomEquip:

                    if (argumentParameters != "") complexObjectData.equipLevelBonus = int.Parse(argumentParameters);

                    break;

                case ObjectDataArgument.ItemInMainHand:
                    complexObjectData.itemInMainHand = SimpleObjectsDatabase.GetObjectData(argumentParameters.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>());
                    break;

                case ObjectDataArgument.ItemInOffHand:
                    complexObjectData.itemInOffHand = SimpleObjectsDatabase.GetObjectData(argumentParameters.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>());

                    break;

                case ObjectDataArgument.ItemOnHead:
                    complexObjectData.itemOnHead = SimpleObjectsDatabase.GetObjectData(argumentParameters.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>());

                    break;

                case ObjectDataArgument.ItemOnChest:
                    complexObjectData.itemOnChest = SimpleObjectsDatabase.GetObjectData(argumentParameters.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>());
                    break;

                case ObjectDataArgument.ItemsInInventory:
                    var itemsNamesAndNumbers = argumentParameters.Split(',');
                    foreach (var itemAndNumber in itemsNamesAndNumbers)
                    {
                        var itemName = SimpleObjectName.Null;
                        var number = 1;
                        var splitedItemAndNumber = itemAndNumber.Split('*');
                        if (splitedItemAndNumber.Length > 1)
                        {
                            itemName = splitedItemAndNumber[0].DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>();
                            number = splitedItemAndNumber[1].ConcatToInt();
                        }
                        else
                        {
                            itemName = itemAndNumber.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>();
                        }

                        for (int i = 0; i < number; i++)
                        {
                            complexObjectData.itemsInInventory.Add(SimpleObjectsDatabase.GetObjectData(itemName));
                        }
                    }
                    break;

                case ObjectDataArgument.AddTags:
                    foreach (var parameter in argumentParameters.Split(','))
                    {
                        complexObjectData.Body.tagElements.Add(new TagBufferElement(parameter.DecodeCharSeparatedEnumsAndGetFirst<Tag>()));
                    }
                    break;

                case ObjectDataArgument.RemoveTags:
                    foreach (var parameter in argumentParameters.Split(','))
                    {
                        complexObjectData.Body.tagElements.Add(new TagBufferElement(parameter.DecodeCharSeparatedEnumsAndGetFirst<Tag>()));
                    }
                    break;

                case ObjectDataArgument.Tags:
                    complexObjectData.Body.tagElements.Clear();
                    foreach (var parameter in argumentParameters.Split(','))
                    {
                        complexObjectData.Body.tagElements.Add(new TagBufferElement(parameter.DecodeCharSeparatedEnumsAndGetFirst<Tag>()));
                    }
                    break;

                case ObjectDataArgument.XpOnDeath:
                    var creatureComponent = complexObjectData.creatureComponent.Component;
                    creatureComponent.xpOnDeath = int.Parse(argumentParameters);
                    complexObjectData.creatureComponent = new ComponentReferece<CreatureComponent>(creatureComponent);
                    break;

                default:
                    break;
            }
        }
    }

    public static PackData<T> GetGenericPackData<T>(SpawnData<T> spawnData, int count) where T : DeepClonable
    {
        var genericPackData = new PackData<T>(PackName.Any);

        for (int i = 0; i < count; i++)
        {
            genericPackData.members.Add(spawnData.Data.DeepClone<T>());
        }
        return genericPackData;
    }
}