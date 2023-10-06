using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

public struct SlaverComponent : IComponentData
{
    public Entity self;

    public SlaverComponent(Entity self)
    {
        this.self = self;
    }

    public List<Entity> GetSlaves()
    {
        var squadmates = Registers.SquadsRegister.GetSquadmates(self);
        return squadmates.Where(e => e.HasComponent<SlaveTag>()).ToList();
    }

}
