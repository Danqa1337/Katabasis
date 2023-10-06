using System;
using Unity.Entities;

[DisableAutoCreation]
public partial class DecaySystem : MySystemBase
{
    public const int baseDecayTime = 5000;

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, RendererComponent rendererComponent, ref DecayComponent decayComponent) =>
        {
            decayComponent.decayTimer--;
            if (decayComponent.decayTimer < 0)
            {
                rendererComponent.BecomeDecayed();
            }
        }).WithoutBurst().Run();
    }
}

public struct DecayableTag : IComponentData
{
}
[Serializable]
public struct DecayComponent : IComponentData
{
    public int decayTimer;
    public bool isDecayed => decayTimer < 0;

    public DecayComponent(int decayTimer)
    {
        this.decayTimer = decayTimer;
    }
}