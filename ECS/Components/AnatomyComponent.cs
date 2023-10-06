using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
public struct AnatomyComponent : IComponentData
{
    public Entity Body;
    public Entity Head;
    public Entity LowerBody;
    public Entity RightFrontLeg;
    public Entity LeftFrontLeg;
    public Entity RightRearLeg;
    public Entity LeftRearLeg;
    public Entity RightArm;
    public Entity LeftArm;
    public Entity RightClaw;
    public Entity LeftClaw;
    public Entity Tentacle0;
    public Entity Tentacle1;
    public Entity Tentacle2;
    public Entity Tentacle3;
    public Entity Tentacle4;
    public Entity Tail;

    public AnatomyComponent(Entity body) : this()
    {
        Body = body;
    }

    public Entity GetUnarmedAttackBodyPart()
    {
        if(RightArm != Entity.Null)
        {
            return RightArm;
        }
        else
        {
            if(LeftArm != Entity.Null)
            {
                return LeftArm;
            }
            else
            {
                if(Head != Entity.Null)
                {
                    return Head;
                }
                return Body;
            }
        }
    }
    public bool CanHold(EquipTag equipTag)
    {
        switch (equipTag)
        {
            case EquipTag.Weapon:
                return (RightArm != Entity.Null);
                break;
            case EquipTag.Shield:
                return (LeftArm != Entity.Null);
                break;
            case EquipTag.Headwear:
                return (Head != Entity.Null);
                break;
            case EquipTag.Chestplate:
                return (Body != Entity.Null);
                break;
            case EquipTag.None:
                return false;
                break;
        }
        return false;
    }

    public List<Entity> GetBodyPartsNotNull()
    {
        return GetBodyParts().Where(p => p != Entity.Null).ToList();
    }
    public HashSet<Entity> GetBodyParts()
    {

        var hashSet = new HashSet<Entity>();
        foreach (var field in GetType().GetFields())
        {
            if (field.FieldType == typeof(Entity))
            {
                hashSet.Add((Entity)field.GetValue(this));
            }
        }
        return hashSet;
    }

    public Entity GetBodyPart(BodyPartTag bodyPartTag)
    {
        foreach (var item in GetBodyPartsNotNull())
        {
            if (item.GetComponentData<BodyPartComponent>().bodyPartTag == bodyPartTag)
            {
                return item;
            }
        }
        return Entity.Null;
    }
}