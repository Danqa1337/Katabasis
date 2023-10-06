using Perks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

[Binding]
public class AbilitiesHotbar : Singleton<AbilitiesHotbar>, INotifyPropertyChanged
{
    private GridLayoutGroup _gridLayout;

    public event PropertyChangedEventHandler PropertyChanged;

    private string _targetDescription = "";
    private AbilityName _selectedAbility;
    public static Action<AbilityName> OnAbilityUsed;

    [Binding]
    public string TargetDescription
    {
        get
        {
            return _targetDescription;
        }
        set
        {
            _targetDescription = value;
            InvokePropertyChange("TargetDescription");
        }
    }

    private void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }

    private void OnEnable()
    {
        Spawner.OnPlayersSquadSpawned += UpdateAvaibleAbilities;
        PerksTree.OnPerkGranted += ListenPerks;
        PerksTree.OnPerkRevoked += ListenPerks;
        TimeController.OnTurnEnd += UpdateUsability;
        Controller.OnControllerActionInvoked += ListenInput;
        UiManager.OnShowCanvas += OnCanvasShow;
        UiManager.OnHideCanvas += OnCanvasHide;
    }

    private void OnDisable()
    {
        Spawner.OnPlayersSquadSpawned -= UpdateAvaibleAbilities;
        PerksTree.OnPerkGranted -= ListenPerks;
        PerksTree.OnPerkRevoked -= ListenPerks;
        TimeController.OnTurnEnd -= UpdateUsability;
        Controller.OnControllerActionInvoked -= ListenInput;
        UiManager.OnShowCanvas -= OnCanvasShow;
        UiManager.OnHideCanvas -= OnCanvasHide;
    }

    private void ListenPerks(PerkName perkName, Entity entity)
    {
        if (PerksDatabase.GetPerk(perkName) is AbilityGrantingPerk)
        {
            UpdateAvaibleAbilities();
        }
    }

    private void OnCanvasShow(UIName uIName)
    {
        if (uIName == UIName.Anatomy)
        {
            CancelAbility();
            UpdateUsability();
        }
    }

    private void OnCanvasHide(UIName uIName)
    {
        if (uIName == UIName.Anatomy)
        {
            UpdateUsability();
        }
    }

    private void Awake()
    {
        TargetDescription = "";
        _gridLayout = GetComponentInChildren<GridLayoutGroup>();
    }

    private void ListenInput(InputContext inputContext)
    {
        if (inputContext.Action == ControllerActionName.SubmitAbility)
        {
            SubmitAbility();
        }
        if (inputContext.Action == ControllerActionName.CancelAbility)
        {
            CancelAbility();
        }
    }

    public static void UpdateAvaibleAbilities()
    {
        Clear();
        var avaibleAbilities = Player.PlayerEntity.GetBuffer<AbilityElement>();

        foreach (var item in avaibleAbilities)
        {
            var abilitydata = AbilitiesDatabase.GetAbilityData(item.ability);

            if (abilitydata.DisplayInHotbar)
            {
                var abilityIcon = Pooler.Take("AbilityIcon", Vector3.zero).GetComponent<AbilityIcon>();
                var iconSprite = IconDataBase.GetAbilityIcon(item.ability);

                abilityIcon.abilityData = abilitydata;
                abilityIcon.flicker.Deactivate();
                abilityIcon.transform.SetParent(instance._gridLayout.transform);
                abilityIcon.image.sprite = iconSprite;
                abilityIcon.OnLeftClickEvent.RemoveAllListeners();
                abilityIcon.OnPointerHovering.RemoveAllListeners();
                abilityIcon.OnPointerStopHovering.RemoveAllListeners();
                abilityIcon.CostGnosisLabel.SetIntValue = abilitydata.GnosisCost;
            }
        }
        instance._gridLayout.CalculateLayoutInputHorizontal();
        UpdateUsability();
    }

    public static void UpdateUsability()
    {
        foreach (var abilityIcon in instance.gameObject.GetComponentsInChildren<AbilityIcon>())
        {
            var abilitydata = abilityIcon.abilityData;
            var canUse = GnosisManager.CurrentGnosis >= abilitydata.GnosisCost;
            abilityIcon.interactable = canUse;
            abilityIcon.OnLeftClickEvent.RemoveAllListeners();
            if (canUse)
            {
                abilityIcon.OnLeftClickEvent.AddListener(delegate
                {
                    if ((!UiManager.IsUIOpened(UIName.Anatomy) || abilitydata.AbilityTargeting == Abilities.AbilityTargeting.OnSelf) && Controller.IsInputEnabled)
                    {
                        instance.OnClickDelegate(abilityIcon, abilitydata.AbilityName);
                    }
                });
            }
        }
    }

    private static void Clear()
    {
        foreach (var child in instance.GetComponentsInChildren<PoolableItem>())
        {
            Pooler.Put(child);
        }
    }

    private void OnClickDelegate(AbilityIcon abilityIcon, AbilityName abilityName)
    {
        if (instance._selectedAbility != AbilityName.Null)
        {
            CancelAbility();
        }
        else
        {
            var abilityData = AbilitiesDatabase.GetAbilityData(abilityName);
            instance._selectedAbility = abilityName;
            abilityIcon.flicker.Activate();
            OnAbilityUsed?.Invoke(abilityName);

            switch ((abilityData.AbilityTargeting))
            {
                case Abilities.AbilityTargeting.Instant:
                    SubmitAbility();
                    break;

                case Abilities.AbilityTargeting.OnTile:
                    ChooseAbilityTarget(abilityData.AbilityName);
                    break;

                case Abilities.AbilityTargeting.OnEntity:
                    ChooseAbilityTarget(abilityData.AbilityName);
                    break;

                case Abilities.AbilityTargeting.OnSelf:
                    AnatomyScreen.ApplyAbilityAction = ApplyActionOnEntity;
                    UiManager.ShowUiCanvas(UIName.Anatomy);

                    void ApplyActionOnEntity(Entity entity)
                    {
                        Player.PlayerEntity.AddComponentData(new AbilityComponent(abilityName, Player.CurrentTile, entity));
                        UiManager.HideUiCanvas(UIName.Anatomy);
                        ManualSystemUpdater.Update<AbilitySystem>();
                    }
                    break;

                case Abilities.AbilityTargeting.OrderOnTile:
                    goto case Abilities.AbilityTargeting.OnTile;
                default:
                    break;
            }
        }
    }

    public static void ChooseAbilityTarget(AbilityName ability)
    {
        Controller.ChangeActionMap(ActionMap.AbilityAim);
        AdjustMask(ability);
    }

    public static void SubmitAbility()
    {
        var abilitydata = AbilitiesDatabase.GetAbilityData(instance._selectedAbility);
        var abilityComponent = GetAbilityComponent(abilitydata, Selector.SelectedTile);

        CancelAbility();
        if (Selector.SelectedTile != TileData.Null && GetTileFunc(abilitydata)(Selector.SelectedTile))
        {
            Debug.Log("Player used ability: " + instance._selectedAbility);

            switch (abilitydata.AbilityTargeting)
            {
                case Abilities.AbilityTargeting.OnEntity:
                    SubmitAbilityOnEntity();
                    break;

                case Abilities.AbilityTargeting.OrderOnTile:
                    SubmitOrderOnTile();
                    break;

                default:
                    SubmitDefault();
                    break;
            }

            ManualSystemUpdater.Update<AbilitySystem>();
        }

        UpdateUsability();

        void SubmitAbilityOnEntity()
        {
            UiManager.ShowUiCanvas(UIName.Aiming);
            AimScreen.Aim(abilityComponent.targetTile.SolidLayer, ApplyActionOnEntity, abilitydata.ShowColorsInAim, abilitydata.ShowEquipInAim);

            void ApplyActionOnEntity(Entity entity, TileData tileData)
            {
                Player.PlayerEntity.AddComponentData(new AbilityComponent(abilitydata.AbilityName, tileData, entity));
            }
        }
        void SubmitDefault()
        {
            Player.PlayerEntity.AddComponentData(abilityComponent);
        }
        void SubmitOrderOnTile()
        {
            SubmitDefault();
        }
    }

    private static Func<TileData, bool> GetTileFunc(AbilityData abilityData)
    {
        return t => abilityData.Ability.TileFunc(Player.PlayerEntity, GetAbilityComponent(abilityData, t));
    }

    private static AbilityComponent GetAbilityComponent(AbilityData abilityData, TileData tileData)
    {
        switch (abilityData.AbilityTargeting)
        {
            default:
                return new AbilityComponent(abilityData.AbilityName, tileData);
        }
    }

    public static void CancelAbility()
    {
        instance._selectedAbility = AbilityName.Null;
        Controller.ChangeActionMap(ActionMap.Default);
        SelectionMask.ResetMask();
        instance.TargetDescription = "";
        foreach (var item in instance.GetComponentsInChildren<UiFlicker>())
        {
            item.Deactivate();
        }
    }

    private static void AdjustMask(AbilityName abilityName)
    {
        var abilityData = AbilitiesDatabase.GetAbilityData(abilityName);
        var tileFound = false;
        SelectionMask.SetSelectionMask(new SelectionMask.SelectionMaskSettings(GetTileFunc(abilityData)), out tileFound);

        instance.TargetDescription = LocalizationManager.GetString(tileFound ? "TargetDescriptionFound" : "TargetDescriptionFoundNoTarget");
    }
}