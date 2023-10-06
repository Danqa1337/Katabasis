using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;

public class MainCameraHandler : Singleton<MainCameraHandler>
{
    public static Camera MainCamera
    {
        get
        {
            return instance._cam;
        }
    }

    public enum MainCameraMode
    {
        FlowCoursor,
        FolowPlayer,
    }

    public float zoomStepOrpho;
    public float maxOrphoSize;
    public float minOrphoSize;
    public float speed;
    public float moveTrashold;
    public bool folow;
    private static bool windowSelected;

    [SerializeField] private Camera _cam;
    [SerializeField] private PixelPerfectCamera _pixelPerfectCamera;

    private void OnEnable()
    {
        Controller.OnControllerActionInvoked += ListenInput;
        Spawner.OnPlayerSpawned += ListenPlayerSpawned;
    }

    private void OnDisable()
    {
        Controller.OnControllerActionInvoked -= ListenInput;
        Spawner.OnPlayerSpawned -= ListenPlayerSpawned;
    }

    private void OnApplicationFocus(bool focus)
    {
        windowSelected = focus;
    }

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        //Spawner.OnPlayerSpawned += attachToPlayer;
    }

    private void Update()
    {
        if (folow && Controller.SelectionEnabled && LocationMap.MapRefference.IsCreated && windowSelected)
        {
            var screenSize = new Vector2(Screen.width, Screen.height);
            var mouseVector = (Mouse.current.position.ReadValue() - screenSize * 0.5f);
            if (Math.Abs(mouseVector.x) > screenSize.x * moveTrashold || Math.Abs(mouseVector.y) > screenSize.y * moveTrashold)
            {
                var newPosition = transform.position + mouseVector.normalized.ToVector3() * speed * PlayerSettings.instance.cameraMoveSens * Time.deltaTime;
                if (newPosition.ToInt2().ToTileData().valid)
                {
                    transform.position = newPosition;
                }
            }
        }
    }

    public static void CenterCameraOnTile(TileData tileData)
    {
        instance.transform.position = tileData.position.ToRealPosition();
    }

    private void ListenPlayerSpawned(Entity entity)
    {
        transform.position = entity.CurrentTile().position.ToRealPosition();
    }

    private void ListenInput(InputContext inputContext)
    {
        if (inputContext.Action == ControllerActionName.Zoom)
        {
            if (!inputContext.IsPointerOverUi)
            {
                if (inputContext.CallbackContext.ReadValue<float>() > 0)
                {
                    ZoomIn();
                }
                else
                {
                    ZoomOut();
                }
            }
        }
    }

    public void ZoomIn()
    {
        if (_pixelPerfectCamera.enabled)
        {
            ZoomInPixelPerfect();
        }
        else
        {
            ZoomInOrpho();
        }
    }

    public void ZoomOut()
    {
        if (_pixelPerfectCamera.enabled)
        {
            ZoomOutPixelPerfect();
        }
        else
        {
            ZoomOutOrpho();
        }
    }

    private void ZoomInPixelPerfect()
    {
        var screenResolotionX = Screen.currentResolution.width;
        _pixelPerfectCamera.assetsPPU = 32;
        _pixelPerfectCamera.refResolutionX = (int)Mathf.Max(128, _pixelPerfectCamera.refResolutionX * .5f);
        _pixelPerfectCamera.refResolutionY = (int)Mathf.Max(72, _pixelPerfectCamera.refResolutionY * .5f);

        if (_pixelPerfectCamera.refResolutionX > screenResolotionX || _pixelPerfectCamera.assetsPPU < 32)
        {
            _pixelPerfectCamera.assetsPPU *= 2;
        }
        else
        {
            _pixelPerfectCamera.assetsPPU = 32;
            _pixelPerfectCamera.refResolutionX = (int)Mathf.Max(128, _pixelPerfectCamera.refResolutionX * .5f);
            _pixelPerfectCamera.refResolutionY = (int)Mathf.Max(72, _pixelPerfectCamera.refResolutionY * .5f);
        }
    }

    private void ZoomOutPixelPerfect()
    {
        var screenResolotionX = Screen.currentResolution.width;
        var screenResolotionY = Screen.currentResolution.height;

        if (_pixelPerfectCamera.refResolutionX >= screenResolotionX && _pixelPerfectCamera.assetsPPU > 8)
        {
            _pixelPerfectCamera.assetsPPU /= 2;
        }
        else
        {
            _pixelPerfectCamera.refResolutionX = Mathf.Min(screenResolotionX, _pixelPerfectCamera.refResolutionX * 2);
            _pixelPerfectCamera.refResolutionY = Mathf.Min(screenResolotionY, _pixelPerfectCamera.refResolutionY * 2);
        }
    }

    private void ZoomInOrpho()
    {
        MainCamera.orthographicSize = Mathf.Clamp(MainCamera.orthographicSize - zoomStepOrpho * PlayerSettings.instance.zoomSens, minOrphoSize, maxOrphoSize);
    }

    private void ZoomOutOrpho()
    {
        MainCamera.orthographicSize = Mathf.Clamp(MainCamera.orthographicSize + zoomStepOrpho * PlayerSettings.instance.zoomSens, minOrphoSize, maxOrphoSize);
    }
}