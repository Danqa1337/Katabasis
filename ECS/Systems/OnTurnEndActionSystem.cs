using Unity.Entities;
[DisableAutoCreation]
public partial class OnTurnEndActionSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        Entities.WithAll<DrawHitAnimationTag>().ForEach((Entity entity, RendererComponent renderer) =>
        {
            ecb.RemoveComponent<DrawHitAnimationTag>(entity);
            renderer.DrawHitAnimation();
        }
        ).WithoutBurst().Run();
        UpdateECB();
    }
}
public struct DrawHitAnimationTag : IComponentData
{

}
