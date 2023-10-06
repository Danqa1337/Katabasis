using Unity.Entities;
[System.Serializable]
public struct DoorComponent : IComponentData
{
    public bool Opened;

    public DoorComponent(bool opened)
    {
        Opened = opened;
    }
}