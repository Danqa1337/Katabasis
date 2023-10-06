using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class AnatomyPortrait : MonoBehaviour
{
    public Color LowEvaluationColor { get; set; }
    public Color HighEvaluationColor { get; set; }
    public Color MaxEvaluationColor { get; set; }
    [Range(0f,1f)]
    [SerializeField] private float _evaluationTint = 0.3f;
    [Range(0f, 1f)]
    [SerializeField] private float _alpha = 1;
    [SerializeField] private float2 _outlineRange;
    [SerializeField] private Color _outlineColor;

    public ColorBlock AnatomyPartColorBlock
    {
        get => _anatomyPartsUIs[0].colors;
        set
        {
            foreach (var item in _anatomyPartsUIs)
            {
                item.colors = value;
            }
        }
    }

    public AnatomyPartUI[] anatomyPartUIs => _anatomyPartsUIs;
    private AnatomyPartUI[] _anatomyPartsUIs;

    public Sprite[] anatomySprites;
    public AnatomyPartUI.PartEventHandler OnPointerEnterDelegate;
    public AnatomyPartUI.PartEventHandler OnPointerExitDelegate;
    public AnatomyPartUI.PartEventHandler OnPointerDownDelegate;
    public AnatomyPartUI.PartEventHandler OnPointerUpDelegate;
    public AnatomyPartUI.PartEventHandler OnDropDelegate;
    public AnatomyPartUI.PartEventHandler OnBeginDragDelegate;
    public AnatomyPartUI.PartEventHandler OnEndDragDelegate;
    public AnatomyPartUI.PartEventHandler OnSelectDelegate;
    public AnatomyPartUI.PartEventHandler OnDeselectDelegate;

    private void Start()
    {
        _anatomyPartsUIs = GetComponentsInChildren<AnatomyPartUI>();
        for (int i = 0; i < _anatomyPartsUIs.Length; i++)
        {
            var partUi = _anatomyPartsUIs[i];
            partUi.SetSprite(anatomySprites[i]);

            partUi.OnPointerEnterDelegate += OnPointerEnterDelegate;
            partUi.OnPointerExitDelegate += OnPointerExitDelegate;
            partUi.OnPointerDownDelegate += OnPointerDownDelegate;
            partUi.OnPointerUpDelegate += OnPointerUpDelegate;
            partUi.OnDropDelegate += OnDropDelegate;
            partUi.OnBeginDragDelegate += OnBeginDragDelegate;
            partUi.OnEndDragDelegate += OnEndDragDelegate;
            partUi.OnSelectDelegate += OnSelectDelegate;
            partUi.OnDeselectDelegate += OnDeselectDelegate;
        }
    }

    private void OnDestroy()
    {
        foreach (var partUi in _anatomyPartsUIs)
        {
            partUi.OnPointerEnterDelegate -= OnPointerEnterDelegate;
            partUi.OnPointerExitDelegate -= OnPointerExitDelegate;
            partUi.OnPointerDownDelegate -= OnPointerDownDelegate;
            partUi.OnPointerUpDelegate -= OnPointerUpDelegate;
            partUi.OnDropDelegate -= OnDropDelegate;
            partUi.OnBeginDragDelegate -= OnBeginDragDelegate;
            partUi.OnEndDragDelegate -= OnEndDragDelegate;
            partUi.OnSelectDelegate -= OnSelectDelegate;
            partUi.OnDeselectDelegate -= OnDeselectDelegate;
        }
    }

    public void UpdatePortrait(Entity entity, bool evaluate, bool drawEquip)
    {
        if (!entity.Exists())
        {
            throw new System.NullReferenceException("Portraited entity does ot exist");
        }

        var entitiesToDraw = new List<Entity>();

        if (entity.HasComponent<AnatomyComponent>())
        {
            var anatomy = entity.GetComponentData<AnatomyComponent>();
            var equipment = entity.GetComponentData<EquipmentComponent>();
            entitiesToDraw.AddRange(anatomy.GetBodyPartsNotNull());

            if (drawEquip)
            {
                entitiesToDraw.AddRange(equipment.GetEquipmentNotNull());
            }
        }
        else
        {
            entitiesToDraw.Add(entity);
        }

        foreach (var part in _anatomyPartsUIs)
        {
            part.texture.Clear();
            part.part = Entity.Null;
        }

        for (int i = 0; i < entitiesToDraw.Count; i++)
        {
            var durabilityPercent = entitiesToDraw[i].GetComponentData<DurabilityComponent>().GetDurabilityPercent;
            _anatomyPartsUIs[i].DrawPart(entitiesToDraw[i], evaluate, LowEvaluationColor, HighEvaluationColor, MaxEvaluationColor, durabilityPercent, _evaluationTint, _alpha, _outlineColor, _outlineRange);
        }
    }
}