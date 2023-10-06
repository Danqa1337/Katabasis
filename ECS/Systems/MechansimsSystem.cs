using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class MechansimsSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();

        Entities.ForEach((Entity entity, RendererComponent rendererComponent, ref ObjectTypeComponent objectTypeComponent, in ToggleMechanismTag toggleMechanismTag, in CurrentTileComponent currentTileComponent) =>
        {
            var locked = false;
            if (entity.HasComponent<LockComponent>())
            {
                var lockComponent = entity.GetComponentData<LockComponent>();
                var lockIndex = lockComponent.lockIndex;
                var @lock = Registers.LocksRegister.GetLock(lockIndex);

                if (@lock.locked)
                {
                    foreach (var item in Player.GetAllItems())
                    {
                        if (item.HasComponent<KeyComponent>())
                        {
                            var keyIndex = item.GetComponentData<KeyComponent>().lockIndex;
                            if (keyIndex == lockIndex || keyIndex == 1)
                            {
                                Registers.LocksRegister.UnLock(lockIndex);
                                break;
                            }
                        }
                    }

                    if (@lock.locked)
                    {
                        Debug.Log("No Key for mechanism");
                        locked = true;
                        PopUpCreator.CreatePopUp(currentTileComponent.CurrentTile.position, PopupType.Locked);
                        SoundSystem.ScheduleSound(SoundName.Lock, currentTileComponent.CurrentTile);
                    }
                    else
                    {
                        Debug.Log("Mechanism unlocked");
                        locked = false;
                        PopUpCreator.CreatePopUp(currentTileComponent.CurrentTile.position, PopupType.Unlocked);
                        SoundSystem.ScheduleSound(SoundName.Unlock, currentTileComponent.CurrentTile);
                    }
                }
            }

            if (!locked)
            {
                if (entity.HasComponent<DoorComponent>())
                {
                    var door = entity.GetComponentData<DoorComponent>();
                    var currentTile = currentTileComponent.CurrentTile;

                    if (door.Opened)
                    {
                        if (currentTile.GroundCoverLayer.Contains(entity))
                        {
                            door.Opened = false;
                            rendererComponent.SwitchToNormalSprite();
                            objectTypeComponent.objectType = ObjectType.Solid;
                            SoundSystem.ScheduleSound(SoundName.DoorClose, currentTileComponent.CurrentTile);

                            FOVSystem.ScheduleFOWUpdate();
                        }
                        else
                        {
                            throw new System.ArgumentOutOfRangeException("Door is not in place");
                        }
                    }
                    else
                    {
                        if (entity.CurrentTile().SolidLayer == entity)
                        {
                            door.Opened = true;
                            rendererComponent.SwitchToAltSprite();
                            objectTypeComponent.objectType = ObjectType.GroundCover;
                            SoundSystem.ScheduleSound(SoundName.DoorOpen, currentTileComponent.CurrentTile);

                            FOVSystem.ScheduleFOWUpdate();
                        }
                        else
                        {
                            throw new System.ArgumentOutOfRangeException("Door is not in place");
                        }
                    }

                    currentTile.Remove(entity);
                    currentTile.Add(entity, objectTypeComponent.objectType);
                    ecb.SetComponent(entity, door);
                }
            }

            ecb.RemoveComponent<ToggleMechanismTag>(entity);
        }).WithoutBurst().Run();
        UpdateECB();
    }
}

public struct ToggleMechanismTag : IComponentData
{
}