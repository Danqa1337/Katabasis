using Gods;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GodIcon : KatabasisSelectable
{
    [SerializeField] private Image _topImage;
    [SerializeField] private Image _midImage;
    [SerializeField] private Image _botImage;

    public static event Action<int> OnPointerEnterStatic;

    public static event Action<int> OnLeftClickStatic;

    public static event Action OnPointerExitStatic;

    private int _godIndex;

    public void Clear()
    {
        _topImage.color = Color.clear;
        _midImage.color = Color.clear;
        _botImage.color = Color.clear;
    }

    public void Draw(God god)
    {
        if (god is RandomizedGod)
        {
            Draw(god as RandomizedGod);
        }
        else
        {
            _godIndex = god.Index;
            _topImage.sprite = IconDataBase.GetGodIcon(god.GodArchetype);
        }
    }

    public void Draw(RandomizedGod randomizedGod)
    {
        _godIndex = randomizedGod.Index;

        _topImage.sprite = IconDataBase.GetTopGodIconPart(randomizedGod.IconData.TopGodIconPart.Key);
        _topImage.color = randomizedGod.IconData.TopGodIconPart.Value;

        _midImage.sprite = IconDataBase.GetMidGodIconPart(randomizedGod.IconData.MiddleGodIconPart.Key);
        _midImage.color = randomizedGod.IconData.MiddleGodIconPart.Value;

        _botImage.sprite = IconDataBase.GetBotGodIconPart(randomizedGod.IconData.BottomGodIconPart.Key);
        _botImage.color = randomizedGod.IconData.BottomGodIconPart.Value;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnPointerEnterStatic?.Invoke(_godIndex);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitStatic?.Invoke();
    }

    protected override void OnLeftClick()
    {
        base.OnLeftClick();
        OnLeftClickStatic?.Invoke(_godIndex);
    }
}