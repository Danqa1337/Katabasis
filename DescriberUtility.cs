using Unity.Entities;

public static class DescriberUtility
{
    public static string GetName(Entity entity)
    {
        string name = "unnamed entity";
        if (entity.HasComponent<TileContainerTag>())
        {
            name = LocalizationManager.GetString("GroundContainer");
        }
        if (entity.HasComponent<GroundTag>())
        {
            return "Ground";
        }
        if (entity.HasComponent<SimpleObjectNameComponent>())
        {
            name = LocalizationManager.GetName(entity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName);
        }
        if (entity.HasComponent<ComplexObjectNameComponent>())
        {
            name = LocalizationManager.GetName(entity.GetComponentData<ComplexObjectNameComponent>().complexObjectName);
        }
        var upgrade = entity.HasComponent<UpgradeComponent>() ? entity.GetComponentData<UpgradeComponent>().upgrade : 0;

        if (upgrade > 0) name = name + " +" + upgrade;
        if (upgrade < 0) name = name + " " + upgrade;
        return name;
    }

    public static string GetDescription(Entity entity)
    {
        var desc = "missig description";
        if (entity.HasComponent<SimpleObjectNameComponent>())
        {
            desc = LocalizationManager.GetDescription(entity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName);
        }
        if (entity.HasComponent<ComplexObjectNameComponent>())
        {
            desc = LocalizationManager.GetDescription(entity.GetComponentData<ComplexObjectNameComponent>().complexObjectName);
        }
        return desc;
    }
}