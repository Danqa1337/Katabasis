using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RadialControllerScreen : MonoBehaviour
{
    private RadialLayout _radialLayout;
    private RectTransform _layoutRectTransform;
    private bool _operating;
    [SerializeField] private int _animationFrameDelayMS;
    [SerializeField] private int _animationFrameCount;
    [SerializeField] private float _maxOperationalRadius;
    [SerializeField] private float _maxLayoutFDistance;

    private void OnEnable()
    {
        UiManager.OnShowCanvas += DoOnCanvasShow;
        UiManager.OnHideCanvas += DoOnCanvasHide;
    }

    private void OnDisable()
    {
        UiManager.OnShowCanvas -= DoOnCanvasShow;
        UiManager.OnHideCanvas -= DoOnCanvasHide;
    }

    private void Awake()
    {
        _radialLayout = GetComponentInChildren<RadialLayout>();
        _layoutRectTransform = _radialLayout.GetComponent<RectTransform>();
        _maxLayoutFDistance = _radialLayout.fDistance;
        _radialLayout.fDistance = 0;
    }

    private void Update()
    {
        if (UiManager.IsUIOpened(UIName.RadialMenu) && !_operating && (Mouse.current.position.ReadValue().ToVector3() - _layoutRectTransform.position).magnitude > _maxOperationalRadius)
        {
            UiManager.HideUiCanvas(UIName.RadialMenu);
        }
    }

    protected async void DoOnCanvasShow(UIName uIName)
    {
        if (uIName != UIName.RadialMenu) return;
        _layoutRectTransform.localPosition = MainCameraHandler.MainCamera.WorldToScreenPoint(Selector.SelectedTile.position.ToVector2()) - new Vector2(Screen.width, Screen.height).ToVector3() * 0.5f;

        foreach (var controllerActionName in RadialController.GetActionsThatCanBeInvokedOnTile(Selector.SelectedTile))
        {
            var actionIcon = Pooler.Take<ActionIcon>("ActionIcon");
            actionIcon.transform.SetParent(_radialLayout.transform);
            actionIcon.transform.position = Vector2.zero;
            actionIcon.DrawAction(controllerActionName);

            actionIcon.OnLeftClickEvent.AddListener(delegate
            {
                RadialController.InvokeControllerAction(controllerActionName);
                UiManager.HideUiCanvas(UIName.RadialMenu);
            });
        }
        _operating = true;
        await Task.Delay(_animationFrameDelayMS);

        for (float i = 0; i <= _maxLayoutFDistance; i += _radialLayout.fDistance / _animationFrameCount)
        {
            _radialLayout.fDistance = i;
            _radialLayout.CalculateRadial();

            await Task.Delay(_animationFrameDelayMS);
        }
        _operating = false;
    }

    private void Clear()
    {
        foreach (var child in _layoutRectTransform.GetComponentsInChildren<PoolableItem>())
        {
            Pooler.Put(child);
        }
    }

    protected async void DoOnCanvasHide(UIName uIName)
    {
        if (uIName != UIName.RadialMenu) return;
        if (!_operating)
        {
            _operating = true;
            for (float i = _maxLayoutFDistance; i >= 0; i -= _radialLayout.fDistance / _animationFrameCount)
            {
                _radialLayout.fDistance = i;
                _radialLayout.CalculateRadial();
                await Task.Delay(_animationFrameDelayMS);
            }

            _operating = false;

            Clear();
        }
    }
}