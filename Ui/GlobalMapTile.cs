using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalMapTile : KatabasisSelectable
{
    public Location Location => _location;
    private Location _location;
    private UiFlicker _flicker;
    [SerializeField] private Image _icon;
    public Sprite CaveSprite;
    public Sprite TownSprite;
    public Sprite PitSprite;
    public Sprite LakeSprite;
    public Sprite ArenaSprite;
    public Sprite RockSprite;
    public Sprite UnknownSprite;

    public static event Action<Location> OnPoiterEnterEvent;

    public static event Action OnPoiterExitEvent;

    protected override void Awake()
    {
        _flicker = GetComponent<UiFlicker>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (_location != null)
        {
            OnPoiterEnterEvent?.Invoke(Location);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (_location != null)
        {
            OnPoiterExitEvent?.Invoke();
        }
    }

    public void DrawLocation(Location location)
    {
        _location = location;
        var sprite = RockSprite;
        if (location.GenerationPreset == GenerationPresetName.Dungeon) sprite = CaveSprite;
        if (location.GenerationPreset == GenerationPresetName.Arena) sprite = ArenaSprite;
        if (location.GenerationPreset == GenerationPresetName.Lake) sprite = LakeSprite;
        if (location.GenerationPreset == GenerationPresetName.Pit) sprite = PitSprite;
        if (location.HyperstuctureName == HyperstuctureName.Town) sprite = TownSprite;
        _icon.sprite = sprite;
        if (location.Id == Registers.GlobalMapRegister.CurrentLocation.Id)
        {
            _flicker.Activate();
        }
        else
        {
            _flicker.Deactivate();
        }
    }

    public void DrawRock()
    {
        _location = null;
        _icon.sprite = RockSprite;
        _flicker.Deactivate();
    }

    public void DrawUnknown()
    {
        _location = null;
        _icon.sprite = UnknownSprite;
        _flicker.Activate();
    }
}