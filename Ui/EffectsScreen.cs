using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectsScreen : Singleton<EffectsScreen>
{
    private VerticalLayoutGroup _iconsLayout;
    private List<EffectIcon> _effectIcons;

    private void Awake()
    {
        _iconsLayout = GetComponentInChildren<VerticalLayoutGroup>();
        _effectIcons = new List<EffectIcon>();
    }

    private void OnEnable()
    {
        TimeController.OnTurnEnd += UpdateEffects;
    }

    private void OnDisable()
    {
        TimeController.OnTurnEnd -= UpdateEffects;
    }

    private void ClearEffects()
    {
        var icons = _iconsLayout.transform.GetComponentsInChildren<EffectIcon>();
        for (int i = 0; i < icons.Length; i++)
        {
            Pooler.Put(icons[i].gameObject);
        }
    }

    private void AddIcon(EffectElement effect)
    {
        var obj = Pooler.Take("EffectIcon", Vector3.zero);
        var icon = obj.GetComponent<EffectIcon>();
        obj.transform.SetParent(_iconsLayout.transform);
        icon.UpdateIcon(effect);

        obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        obj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        _effectIcons.Add(obj.GetComponent<EffectIcon>());
    }

    private static void UpdateEffects()
    {
        instance.ClearEffects();
        var activeEffects = new List<EffectElement>();

        foreach (var part in Player.PlayerEntity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
        {
            foreach (var effect in part.GetEffects())
            {
                activeEffects.Add(effect);
            }
        }

        foreach (var effect in activeEffects)
        {
            if (IconDataBase.GetEffectIcon(effect.EffectName) != null)
            {
                instance.AddIcon(effect);
            }
        }
    }
}