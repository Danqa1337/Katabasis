using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum ControllerActionName
{
    NULL,
    TogglePause,
    ToggleInventory,
    CloseInventory,
    ToggleDescriber,
    ToggleMap,
    ToggleContainer,
    ToggleConsole,
    CloseLast,

    MoveU,
    MoveUR,
    MoveR,
    MoveDR,
    MoveD,
    MoveDL,
    MoveL,
    MoveUL,

    Rest,
    Travel,
    WaitForOneTurn,
    WaitForOneTick,

    Eat,
    Grab,
    Throw,
    Jump,
    Explode,
    ActionWithMainhand,
    Kick,
    MakeFullBodySlam,
    GenerateMap,
    AnyKeyPressed,
    EnterStaircase,
    Zoom,
    SwapHands,
    ShowActions,
    HideActions,
    Give,
    ToggleMechanism,
    CancelAbility,
    SubmitAbility,
    LookAtTheFloor,
    OpenContainer,

    LeftClick,
    RightClick,
    Sacrifice,
    Pray,
}

public enum ActionMap
{
    Default,
    Console,
    Menuing,
    Inventory,
    RadialMenu,
    AbilityAim,
}

public class Controller : Singleton<Controller>
{
    [SerializeField] private PlayerInput _playerInput;
    public static Action<InputContext> OnControllerActionInvoked;
    private bool _buttonHeld;
    private bool _isInputEnabled;
    public bool debug;
    private Dictionary<ControllerActionName, Action> _actionsByNames;
    public static bool IsInputEnabled => instance._isInputEnabled;

    public static bool IsPointerOverUi
    {
        get
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Mouse.current.position.ReadValue();
            var raycasts = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycasts);
            for (int i = 0; i < raycasts.Count; i++)
            {
                if (raycasts[i].gameObject.GetComponent<PointerOverIngnoring>() == null)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static bool SelectionEnabled
    {
        get
        {
            if (instance._playerInput.currentActionMap.name == ActionMap.Default.ToString()
                || instance._playerInput.currentActionMap.name == ActionMap.AbilityAim.ToString()) return true;
            return false;
        }
    }

    private void OnEnable()
    {
        _playerInput.onActionTriggered += Listen;
        Player.OnPlayerDied += DisableInput;
        Spawner.OnPlayersSquadSpawned += EnableInput;
        LocationEnterManager.OnExitPrevLocationCompleted += delegate { DisableInput(); };
        LocationEnterManager.OnMovingToLocationCompleted += delegate { EnableInput(); };
        TimeController.OnSpendTime += DisableInput;
        TimeController.OnTurnEnd += EnableInput;
    }

    private void OnDisable()
    {
        _playerInput.onActionTriggered -= Listen;
        Player.OnPlayerDied -= DisableInput;
        Spawner.OnPlayersSquadSpawned -= EnableInput;
        LocationEnterManager.OnExitPrevLocationCompleted -= delegate { DisableInput(); };
        LocationEnterManager.OnMovingToLocationCompleted -= delegate { EnableInput(); };
        TimeController.OnSpendTime -= DisableInput;
        TimeController.OnTurnEnd -= EnableInput;
    }

    private void EnableInput()
    {
        _isInputEnabled = true;
    }

    private void DisableInput()
    {
        _isInputEnabled = false;
    }

    private void Listen(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _buttonHeld = true;
            var actionName = ControllerActionName.NULL;
            if (context.action.name.DecodeCharSeparatedEnumsAndGetFirst<ControllerActionName>(out actionName))
            {
                if (debug)
                {
                    Debug.Log(context.action.name);
                    if (!IsInputEnabled)
                    {
                        Debug.Log("input isnt active");
                    }

                    if (IsPointerOverUi)
                    {
                        Debug.Log("pointer is over ui");
                    }
                }
                if (IsInputEnabled)
                {
                    OnControllerActionInvoked.Invoke(new InputContext(actionName, context, IsPointerOverUi));
                }
            }
        }
        if (context.canceled)
        {
            _buttonHeld = false;
        }
    }

    public static async void ChangeActionMap(ActionMap actionMap)
    {
        if (instance._playerInput != null)
        {
            instance._playerInput.DeactivateInput();
            instance._playerInput.SwitchCurrentActionMap(actionMap.ToString());
            await Task.Delay(100);
            instance._playerInput.ActivateInput();
            // Debug.Log("action map" + instance._playerInput.currentActionMap.name);
        }
    }
}

public class InputContext
{
    public readonly ControllerActionName Action;
    public readonly InputAction.CallbackContext CallbackContext;
    public readonly bool IsPointerOverUi;

    public InputContext(ControllerActionName action, InputAction.CallbackContext callbackContext, bool isPointerOverUi)
    {
        Action = action;
        CallbackContext = callbackContext;
        IsPointerOverUi = isPointerOverUi;
    }
}