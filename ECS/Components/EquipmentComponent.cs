using System.Collections.Generic;
using System.Globalization;
using Unity.Entities;
using Unity.Mathematics;
[System.Serializable]
public struct EquipmentComponent : IComponentData
{
    [System.NonSerialized] public Entity itemInMainHand;
    [System.NonSerialized] public Entity itemInOffHand;
    [System.NonSerialized] public Entity itemOnChest;
    [System.NonSerialized] public Entity itemOnHead;

    public float2 rightArmHolderOffset;
    public float2 leftArmHolderOffset;
    public float2 headHolderOffset;
    public float2 chestHolderOffset;
    public List<EquipTag> GetAllowedEquipTags()
    {
        var list = new List<EquipTag>();

        return list;
    }
    public EquipmentComponent(ComplexObjectsTable.Param param)
    {
        itemInMainHand = Entity.Null;
        itemInOffHand = Entity.Null;
        itemOnChest = Entity.Null;
        itemOnHead = Entity.Null;

        rightArmHolderOffset = ParseOffset(param.MainHandOffset);
        leftArmHolderOffset = ParseOffset(param.OffHandOffset);
        headHolderOffset = ParseOffset(param.HelmetOffset);
        chestHolderOffset = ParseOffset(param.ChestplateOffset);

        float2 ParseOffset(string stringOffset)
        {
            if (stringOffset != "")
            {
                var splited = stringOffset.Split(',');
                int x = 0;
                int y = 0;
                int.TryParse(splited[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
                int.TryParse(splited[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y);

                return new float2(x / 32f, y / 32f);
            }
            else
            {
                return float2.zero;
            }
        }

    }

    public Entity GetEquipmentEntity(EquipTag tag) => tag switch
    {
        EquipTag.Weapon => itemInMainHand,
        EquipTag.Chestplate => itemOnChest,
        EquipTag.Shield => itemInOffHand,
        EquipTag.Headwear => itemOnHead,
        _ => itemInMainHand,

    };
    public EquipTag GetEquipTag(Entity entity)
    {
        if (itemInMainHand == entity) return EquipTag.Weapon;
        if (itemInOffHand == entity) return EquipTag.Shield;
        if (itemOnHead == entity) return EquipTag.Headwear;
        if (itemOnChest == entity) return EquipTag.Chestplate;
        return EquipTag.None;
    }

    public float2 GetOffset(EquipTag tag)
    {
        float2 offset = tag switch
        {
            EquipTag.Weapon => rightArmHolderOffset,
            EquipTag.Shield => leftArmHolderOffset,
            EquipTag.Headwear => headHolderOffset,
            EquipTag.Chestplate => chestHolderOffset,
            EquipTag.None => rightArmHolderOffset
        };

        return offset;

    }


    public List<Entity> GetEquipmentNotNull()
    {
        List<Entity> things = new List<Entity>();
        if (itemOnChest != Entity.Null) things.Add(itemOnChest);
        if (itemOnHead != Entity.Null) things.Add(itemOnHead);
        if (itemInMainHand != Entity.Null) things.Add(itemInMainHand);
        if (itemInOffHand != Entity.Null) things.Add(itemInOffHand);
        return things;
    }
}



