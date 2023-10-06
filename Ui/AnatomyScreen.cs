using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;
using UnityWeld.Binding;
using System;

[Binding]
public class AnatomyScreen : MonoBehaviour, INotifyPropertyChanged
{
    [SerializeField] private Color LowDurabilityColor;
    [SerializeField] private Color HighDurabilityColor;
    [SerializeField] private Color IntactColor;
    [SerializeField] private Toggle showEquip;
    [SerializeField] private Transform effectsHolder;
    [SerializeField] private Camera HudportraitCamera;

    public static event Action<Entity> OnPointerEnterEvent;

    public static event Action OnPointerExitEvent;

    public AnatomyPortrait anatomyPortrait;
    private string _topLabeltext;

    [Binding]
    public string TopLabelText
    {
        get => _topLabeltext;
        set
        {
            _topLabeltext = value;
            InvokePropertyChange("TopLabelText");
        }
    }

    public static Action<Entity> ApplyAbilityAction;

    private void OnEnable()
    {
        TimeController.OnTurnEnd += delegate { UpdateAnatomyView(true, false); };
        Spawner.OnPlayersSquadSpawned += delegate { UpdateAnatomyView(true, false); };
        showEquip.onValueChanged.AddListener(delegate { UpdateAnatomyView(); });
        anatomyPortrait.OnPointerEnterDelegate += OnPointerEnter;
        anatomyPortrait.OnPointerExitDelegate += OnPointerExit;
        anatomyPortrait.OnSelectDelegate += OnSelect;
        anatomyPortrait.OnPointerDownDelegate += OnLeftClick;
        AbilitiesHotbar.OnAbilityUsed += ListenAbility;
        UiManager.OnShowCanvas += DoOnCanvasShow;
        UiManager.OnHideCanvas += DoOnCanvasHide;
        AnatomySystem.OnPlayerAnatomyChanged += UpdateAnatomyViewIfCanvasShown;
        DurabilitySystem.OnPlayersPartDurabilityChanged += UpdateAnatomyViewIfCanvasShown;
        EquipmentSystem.OnPlayerEquipChanged += UpdateAnatomyViewIfCanvasShown;
    }

    private void OnDisable()
    {
        TimeController.OnTurnEnd -= delegate { UpdateAnatomyView(true, false); };
        Spawner.OnPlayersSquadSpawned -= delegate { UpdateAnatomyView(true, false); };
        showEquip.onValueChanged.AddListener(delegate { UpdateAnatomyView(); });
        anatomyPortrait.OnPointerEnterDelegate -= OnPointerEnter;
        anatomyPortrait.OnPointerExitDelegate -= OnPointerExit;
        anatomyPortrait.OnSelectDelegate -= OnSelect;
        anatomyPortrait.OnPointerDownDelegate -= OnLeftClick;
        AbilitiesHotbar.OnAbilityUsed -= ListenAbility;
        UiManager.OnShowCanvas -= DoOnCanvasShow;
        UiManager.OnHideCanvas -= DoOnCanvasHide;
        AnatomySystem.OnPlayerAnatomyChanged -= UpdateAnatomyViewIfCanvasShown;
        DurabilitySystem.OnPlayersPartDurabilityChanged -= UpdateAnatomyViewIfCanvasShown;
        EquipmentSystem.OnPlayerEquipChanged -= UpdateAnatomyViewIfCanvasShown;
    }

    private void ListenAbility(AbilityName abilityName)
    {
        var abilityData = AbilitiesDatabase.GetAbilityData(abilityName);
        if (abilityData.AbilityTargeting == Abilities.AbilityTargeting.OnSelf)
        {
            UpdateAnatomyView();
        }
    }

    public void UpdateAnatomyViewIfCanvasShown()
    {
        if (UiManager.IsUIOpened(UIName.Anatomy))
        {
            UpdateAnatomyView(true, showEquip.isOn);
        }
    }

    public void UpdateAnatomyView()
    {
        UpdateAnatomyView(true, showEquip.isOn);
    }

    public void UpdateHudPortrait()
    {
        HudportraitCamera.enabled = true;
        HudportraitCamera.Render();
        HudportraitCamera.enabled = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }

    public void UpdateAnatomyView(bool drawHealth, bool drawEquip)
    {
        if (ApplyAbilityAction == null)
        {
            TopLabelText = LocalizationManager.GetString("Anatomy");
        }
        else
        {
            TopLabelText = LocalizationManager.GetString("ChooseAbilityTargetPart");
        }

        anatomyPortrait.UpdatePortrait(Player.PlayerEntity, drawHealth, drawEquip);
    }

    public void OnPointerEnter(AnatomyPartUI anatomyPartUI)
    {
        OnPointerEnterEvent?.Invoke(anatomyPartUI.part);
    }

    public void OnPointerExit(AnatomyPartUI anatomyPartUI)
    {
        OnPointerExitEvent?.Invoke();
    }

    public void OnLeftClick(AnatomyPartUI anatomyPartUI)
    {
        if (ApplyAbilityAction != null)
        {
            ApplyAbilityAction.Invoke(anatomyPartUI.part);
        }
    }

    private void OnSelect(AnatomyPartUI anatomyPartUI)
    {
        anatomyPartUI.OnDeselect(null);
    }

    private void DoOnCanvasShow(UIName uIName)
    {
        if (uIName == UIName.Anatomy)
        {
            UpdateAnatomyView(true, false);
        }
    }

    private void DoOnCanvasHide(UIName uIName)
    {
        if (uIName == UIName.Anatomy)
        {
            ApplyAbilityAction = null;
        }
    }
}

public class PlayersAnatomy
{
    public static Action OnAnatomyChaged;

    public static bool CanHold(EquipTag equipTag)
    {
        return Player.AnatomyComponent.CanHold(equipTag);
    }
}