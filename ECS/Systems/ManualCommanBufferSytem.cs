using Unity.Entities;

[DisableAutoCreation]
public partial class ManualCommanBufferSytem : EntityCommandBufferSystem
{
    public static ManualCommanBufferSytem Instance => World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ManualCommanBufferSytem>();

    public static void PlayBack()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ManualCommanBufferSytem>().Update();
    }
}