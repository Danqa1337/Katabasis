using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct AIComponent : IComponentData
{

    public Entity self;
    public Entity target;
    public int abilityCooldown;
    public int targetSearchCooldown;
    public int agressionCooldown;
    public const int minTrylimit = 256;
    public const int maxTryLimit = 256;
    public int tryLimit;
    public bool AbilityReady => abilityCooldown == 0;


    public bool isFleeing => self.HasComponent<MoraleComponent>() && self.GetComponentData<MoraleComponent>().isFleeing;

    public int2 homePoint;
    public AIComponent(Entity self, int cooldown, int2 homePoint)
    {
        this.tryLimit = minTrylimit;
        this.self = self;
        this.abilityCooldown = cooldown;
        this.targetSearchCooldown = 0;
        this.agressionCooldown = 50;
        this.target = Entity.Null;
        this.homePoint = homePoint;
    }
    public HashSet<Tag> GetHostileTags()
    {
        var list = new HashSet<Tag>();
        foreach (var item in self.GetBuffer<EnemyTagBufferElement>())
        {
            list.Add(item.tag);
        }

        return list;


    }


}



