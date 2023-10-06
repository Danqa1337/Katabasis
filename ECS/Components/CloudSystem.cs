using System.Linq;
using Unity.Entities;

[DisableAutoCreation]
public partial class CloudSystem : MySystemBase
{
    private const int cloudMoveCooldown = 10;

    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();
        var random = GetRandom();

        Entities.ForEach((int entityInQueryIndex, Entity entity, ref CloudComponent cloud) =>
        {
            if (cloud.lifeTime != 0)
            {
                if (cloud.lifeTime > 0)
                {
                    cloud.lifeTime--;
                }

                if (cloud.moveCoolDown > 0)
                {
                    cloud.moveCoolDown--;
                }
                else
                {
                    var neibors = entity.CurrentTile().GetNeibors(true);
                    neibors = neibors.Where(n => n.SolidLayer == Entity.Null && n.HoveringLayer == Entity.Null).ToArray();
                    if (neibors.Length > 0)
                    {
                        var tile = neibors.RandomItemThreadSafe(random);
                        ecb.AddComponent(entityInQueryIndex, entity, new MoveComponent(entity.CurrentTile(), tile, MovemetType.Forced));
                    }
                    cloud.moveCoolDown = random.NextInt(cloudMoveCooldown, cloudMoveCooldown * 2);
                }
            }
            else
            {
                ecb.AddComponent(entityInQueryIndex, entity, new DestroyEntityTag());
            }
        }).WithoutBurst().ScheduleParallel();

        UpdateECB();
    }
}

public struct CloudComponent : IComponentData
{
    public int lifeTime;
    public int moveCoolDown;

    public CloudComponent(int lifeTime)
    {
        this.lifeTime = lifeTime;
        this.moveCoolDown = 10;
    }
}