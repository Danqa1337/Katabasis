using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class HealthSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();
        Entities.ForEach((int entityInQueryIndex, Entity entity, in DynamicBuffer<HealthChangedElement> healthChangeBuffer, in CurrentTileComponent currentTileComponent, in SquadMemberComponent squadMemberComponent) =>
        {
            var healthChange = 0;
            foreach (var item in healthChangeBuffer)
            {
                healthChange += item.value;
                if (item.value < 0)
                {
                    if (item.responsibleEntity.HasComponent<SquadMemberComponent>())
                    {
                        Registers.SquadsRegister.AddEnemyIndex(squadMemberComponent.squadIndex,
                            item.responsibleEntity.GetComponentData<SquadMemberComponent>().squadIndex);
                    }
                }
            }

            Color color = healthChange < 0 ? PopUpCreator.instance.damagePopupColor : PopUpCreator.instance.healPopupColor;
            
            PopUpCreator.CreatePopUp(currentTileComponent.currentTileId.ToMapPosition(), healthChange.ToString(), color);

            if (healthChange < 0 && (entity.HasComponent<TraderComponent>() || entity.HasComponent<TraderComponent>()))
            {
                ecb.AddComponent(entityInQueryIndex, entity, new AbilityComponent(AbilityName.TeleportAway));
            }

            ecb.RemoveComponent<HealthChangedElement>(entityInQueryIndex, entity);



        }).WithoutBurst().Run();
        UpdateECB();
    }
}

public struct HealthChangedElement : IBufferElementData
{
    public int value;
    public Entity responsibleEntity;

    public HealthChangedElement(int value, Entity responsibleEntity)
    {
        this.value = value;
        this.responsibleEntity = responsibleEntity;
    }
}