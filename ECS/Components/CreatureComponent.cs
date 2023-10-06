using Unity.Entities;
[System.Serializable]
public struct CreatureComponent : IComponentData
{
    public int str;
    public int agl;
    public int baseMovementCost;
    public int baseViewDistance;
    public int unarmedAttackDamageBonus;
    public int unarmedAttacCost;
    public Gender gender;

    public int xpOnDeath;

    

    public CreatureComponent(ComplexObjectsTable.Param param)
    {
        baseViewDistance = param.viewDistance;
        str = param.STR;
        agl = param.AGL;
        gender = param.gender.DecodeCharSeparatedEnumsAndGetFirst<Gender>();
        baseMovementCost = param.movementCost;
        xpOnDeath = param.expOnDeath;
        unarmedAttackDamageBonus = param.UnarmedAtackDamage;
        unarmedAttacCost = param.UnarmedAtackCost;
    }

}
public struct UnarmedaAttackEntity : IComponentData
{

}