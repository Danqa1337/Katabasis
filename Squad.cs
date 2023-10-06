using System.Collections.Generic;
using Unity.Entities;

[System.Serializable]
public class Squad
{
    [System.NonSerialized]
    public List<Entity> members = new List<Entity>();

    public List<int> enemySquadIndexes = new List<int>();
    public int Count => members.Count;

    public void Add(Entity entity) => members.Add(entity);

    public void Remove(Entity entity) => members.Remove(entity);

    public Entity Leader
    {
        get
        {
            if (members.Count > 0)
            {
                return members[0];
            }
            else
            {
                return Entity.Null;
            }
        }

        set
        {
            var newMembers = new List<Entity>();
            newMembers.Add(value);
            members.Remove(value);
            newMembers.AddRange(members);
            members = newMembers;
        }
    }

    public Squad(List<Entity> members)
    {
        this.members = members;
    }

    public Squad()
    {
    }
}
