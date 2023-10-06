using Unity.Entities;
[System.Serializable]
public struct SquadMemberComponent : IComponentData
{
    public int squadIndex;
    public SquadMemberComponent(int squadIndex)
    {
        this.squadIndex = squadIndex;
    }
}
