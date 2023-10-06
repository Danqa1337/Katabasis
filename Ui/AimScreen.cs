using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using UnityWeld.Binding;
using UnityEngine.UI;

[Binding]
public class AimScreen : Singleton<AimScreen>, INotifyPropertyChanged
{
    public AnatomyPortrait _anatomyPortrait;
    private static Entity _entityInAim;

    public event PropertyChangedEventHandler PropertyChanged;

    public Toggle _showEquipToggle;
    private float cameraOrphoSize;
    private Vector3 cameraPosition;
    private static Action<Entity, TileData> _currentAction;
    [SerializeField] private Label AttackCostLabel;

    public static event Action<Entity, bool> OnPointerEnterEvent;

    public static event Action OnPointerExitEvent;

    private void Awake()
    {
        _anatomyPortrait.OnPointerDownDelegate += OnPointerDown;
        _anatomyPortrait.OnPointerEnterDelegate += OnPointerEnter;
        _anatomyPortrait.OnPointerExitDelegate += OnPointerExit;
        _showEquipToggle.onValueChanged.AddListener(delegate { UpdatePortrait(); });
    }

    public static void Aim(Entity entity, Action<Entity, TileData> action, bool showCollors = false, bool showEquip = false)
    {
        _entityInAim = entity;
        _currentAction = action;

        instance._showEquipToggle.isOn = false;
        instance.UpdatePortrait();

        MainCameraHandler.CenterCameraOnTile(entity.CurrentTile());
        MainCameraHandler.MainCamera.orthographicSize = MainCameraHandler.instance.minOrphoSize;
    }

    private void UpdatePortrait()
    {
        instance._anatomyPortrait.UpdatePortrait(_entityInAim, true, instance._showEquipToggle.isOn);
    }

    private void OnPointerEnter(AnatomyPartUI anatomyPartUI)
    {
        OnPointerEnterEvent?.Invoke(anatomyPartUI.part, _entityInAim.IsPlayersSquadmate());
    }

    private void OnPointerExit(AnatomyPartUI anatomyPartUI)
    {
        OnPointerExitEvent?.Invoke();
    }

    private async void OnPointerDown(AnatomyPartUI anatomyPartUI)
    {
        var weapon = Player.PlayerEntity.GetComponentData<EquipmentComponent>().itemInMainHand;
        if (weapon == Entity.Null) weapon = Player.PlayerEntity.GetComponentData<AnatomyComponent>().GetUnarmedAttackBodyPart();
        UiManager.HideUiCanvas(UIName.Aiming);
        _currentAction.Invoke(anatomyPartUI.part, Selector.SelectedTile);
        await Task.Delay(250);

        _entityInAim = Entity.Null;
        ManualSystemUpdater.Update<AbilitySystem>();
    }

    private void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }
}