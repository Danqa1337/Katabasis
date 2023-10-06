using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class SwapSystem : MySystemBase
{

    protected override void OnUpdate()
    {

        var query = EntityManager.CreateEntityQuery(ComponentType.ReadWrite<IsGoingToSwapComponent>());
        var array = query.ToEntityArray(Allocator.Temp);
        var ecb = CreateEntityCommandBuffer();

        foreach (var firstEntity in array)
        {
            ecb.RemoveComponent<IsGoingToSwapComponent>(firstEntity);
            var swapComponent = firstEntity.GetComponentData<IsGoingToSwapComponent>();
            var secondEntity = swapComponent.EntityToSwapWith;

            if (firstEntity == secondEntity) throw new Exception("trying to swap with itself");

            if (firstEntity.HasComponent<AllreadySwapedTag>() || firstEntity.HasComponent<MoveComponent>()) continue;
            if (secondEntity.HasComponent<AllreadySwapedTag>() || secondEntity.HasComponent<MoveComponent>()) continue;

            var firstCurrentTileComponent = firstEntity.GetComponentData<CurrentTileComponent>();
            var secondCurrentTileComponent = secondEntity.GetComponentData<CurrentTileComponent>();

            var firstTile = firstCurrentTileComponent.CurrentTile;
            var secondTile = secondCurrentTileComponent.CurrentTile;

            var firstAnimation = new MajorAnimationElement(firstTile, secondTile, AnimationType.Step);
            var SecondAnimation = new MajorAnimationElement(secondTile, firstTile, AnimationType.Step);

            firstTile.SolidLayer = secondEntity;
            secondTile.SolidLayer = firstEntity;

            firstTile.Save();
            secondTile.Save();

            secondEntity.SetComponentData(firstCurrentTileComponent);
            firstEntity.SetComponentData(secondCurrentTileComponent);

            firstEntity.AddBufferElement(firstAnimation);
            secondEntity.AddBufferElement(SecondAnimation);

            firstEntity.SetZeroSizedTagComponentData(new AllreadySwapedTag());
            secondEntity.SetZeroSizedTagComponentData(new AllreadySwapedTag());

            ecb.RemoveComponent<AllreadySwapedTag>(firstEntity);
            ecb.RemoveComponent<AllreadySwapedTag>(secondEntity);

            if (firstEntity.IsPlayer() || Registers.SquadsRegister.AreSquadmates(firstEntity, Player.PlayerEntity))
            {
                FOVSystem.ScheduleFOWUpdate();
            }

            Debug.Log(firstEntity.GetName() + " swaps with " + swapComponent.EntityToSwapWith.GetName());

        }


        UpdateECB();



        array.Dispose();



    }
    private struct AllreadySwapedTag : IComponentData
    {

    }

}
