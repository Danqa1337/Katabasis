using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

public static class RadialController
{
    public static Action<InputContext> OnControllerActionInvoked;

    public static void InvokeControllerAction(ControllerActionName controllerActionName)
    {
        var context = new UnityEngine.InputSystem.InputAction.CallbackContext();
        OnControllerActionInvoked.Invoke(new InputContext(controllerActionName, context, false));
    }

    public static ControllerActionName[] GetActionsThatCanBeInvokedOnTile(TileData tileData)
    {
        var result = new List<ControllerActionName>();
        foreach (ControllerActionName controllerAction in Enum.GetValues(typeof(ControllerActionName)))
        {
            if (CanInvokeActionOnTile(controllerAction, tileData))
            {
                result.Add(controllerAction);
            }
        }
        return result.ToArray();
    }

    private static bool CanInvokeActionOnTile(ControllerActionName controllerAction, TileData tileData)
    {
        if (tileData.maped)
        {
            switch (controllerAction)
            {
                case ControllerActionName.Travel:
                    return tileData != Player.CurrentTile && Selector.Path.Length > 0;

                case ControllerActionName.ToggleDescriber:
                    return tileData.visible;

                case ControllerActionName.ToggleMechanism:
                    if (tileData.IsInRangeOfOne(Player.CurrentTile))
                    {
                        if (tileData.SolidLayer.HasComponent<MechanismTag>())
                        {
                            return true;
                        }
                        else
                        {
                            foreach (var item in tileData.GroundCoverLayer)
                            {
                                if (item.HasComponent<MechanismTag>())
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;

                case ControllerActionName.WaitForOneTurn:
                    return tileData == Player.CurrentTile;

                case ControllerActionName.Eat:
                    return tileData == Player.CurrentTile
                        && tileData.DropLayer.Any(d => d.HasComponent<EatableComponent>());

                case ControllerActionName.Grab:
                    return tileData == Player.CurrentTile
                        && tileData.DropLayer.Any(d => !d.HasComponent<AnatomyComponent>()
                        || Player.EquipmentComponent.itemInMainHand == Entity.Null);

                case ControllerActionName.Throw:
                    return tileData.visible
                        && tileData != Player.CurrentTile
                        && !tileData.IsInRangeOfOne(Player.CurrentTile)
                        && Player.EquipmentComponent.itemInMainHand != Entity.Null;

                case ControllerActionName.Give:
                    return tileData.IsInRangeOfOne(Player.CurrentTile) && tileData.SolidLayer.HasComponent<AliveTag>() && Player.EquipmentComponent.itemInMainHand != Entity.Null;

                case ControllerActionName.Jump:
                    return tileData.SolidLayer == Entity.Null
                        && (tileData.IsInRangeOfOne(Player.CurrentTile) || tileData.IsInRangeOfTwo(Player.CurrentTile));

                case ControllerActionName.ActionWithMainhand:
                    return tileData != Player.CurrentTile
                        && tileData.IsInRangeOfOne(Player.CurrentTile)
                        || (tileData.IsInRangeOfTwo(Player.CurrentTile) && Player.EquipmentComponent.itemInMainHand.HasComponent<PolearmTag>());

                case ControllerActionName.EnterStaircase:
                    foreach (var id in Registers.GlobalMapRegister.CurrentLocation.TransitionsIDs)
                    {
                        var transition = Registers.GlobalMapRegister.GetTransition(id);
                        if (transition.exitPosition.Equals(tileData.position) || transition.entrancePosition.Equals(tileData.position))
                        {
                            return true;
                        }
                    }
                    return false;

                case ControllerActionName.OpenContainer:
                    return tileData.SolidLayer.HasComponent<ContainerComponent>() && tileData.IsInRangeOfOne(Player.CurrentTile);

                default: return false;
            }
        }
        return false;
    }
}