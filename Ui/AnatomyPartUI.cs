using Unity.Entities;
using Unity.Transforms;
using UnityAsync;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnatomyPartUI : KatabasisSelectable, IBeginDragHandler, IDropHandler
{
    public Entity part;

    [HideInInspector] public Image partImage;

    [HideInInspector] public Canvas canvas;
    private RectTransform _parentRectTransform;
    private Outline _outline;

    public Texture2D texture
    {
        get => partImage.sprite.texture;
    }

    public bool Flickering
    {
        get => _flicker.Flickering;
        set
        {
            if (value)
            {
                _flicker.Activate();
            }
            else
            {
                _flicker.Deactivate();
            }
        }
    }

    public Color Color
    {
        get => image.color;
        set
        {
            image.color = value;
        }
    }

    private UiFlicker _flicker;

    public RectTransform rectTransform;

    public PartEventHandler OnPointerEnterDelegate;
    public PartEventHandler OnPointerExitDelegate;

    public PartEventHandler OnPointerDownDelegate;
    public PartEventHandler OnPointerUpDelegate;

    public PartEventHandler OnBeginDragDelegate;
    public PartEventHandler OnEndDragDelegate;

    public PartEventHandler OnSelectDelegate;
    public PartEventHandler OnDeselectDelegate;

    public PartEventHandler OnDropDelegate;

    public delegate void PartEventHandler(AnatomyPartUI anatomyPartUI);

    public void SetSprite(Sprite sprite)
    {
        partImage.sprite = sprite;
    }

    public void DrawPart(Entity entity, bool evaluate, Color LowEvaluationColor, Color HighEvaluationColor, Color IntactColor, float evaluatedParameter, float evaluationTint, float alpha, Color outlineColor, Vector2 outlineRange)
    {
        var renderer = entity.GetComponentObject<RendererComponent>();
        var rect = renderer.Sprite.textureRect;
        var entityTexture = entity.GetComponentObject<RendererComponent>().Sprite.texture;
        var pixelOffset = 64 / 2 - (int)rect.width / 2;
        var entityTransform = entity.GetComponentObject<EntityAuthoring>().transform;
        var globalScale = _parentRectTransform.sizeDelta.y;
        var localScale = globalScale * (32f / rect.width);
        var imagePositionOffset = entity.HasComponent<Parent>() ? new Vector3(entityTransform.localPosition.x, entityTransform.transform.localPosition.y) * localScale / 2f : Vector3.zero;

        _outline.effectColor = outlineColor;
        _outline.effectDistance = outlineRange;

        if (globalScale > localScale) globalScale = localScale;

        partImage.rectTransform.anchoredPosition = imagePositionOffset;
        partImage.rectTransform.ForceUpdateRectTransforms();

        var durabilityTint = IntactColor;

        for (int x = 0; x < rect.width; x++)
        {
            for (int y = 0; y < rect.height; y++)
            {
                var color = entityTexture.GetPixel((int)rect.xMin + x, (int)rect.yMin + y);
                if (evaluate && color.a > 0)
                {
                    if (evaluatedParameter != 100)
                    {
                        durabilityTint = Color.Lerp(LowEvaluationColor, HighEvaluationColor, evaluatedParameter / 100);
                    }
                    if (evaluatedParameter != 100) durabilityTint *= UnityEngine.Random.Range(0.95f, 1.05f);
                    color = Color.Lerp(durabilityTint, color, evaluationTint);
                    color.a = alpha;
                }
                texture.SetPixel(pixelOffset + x, pixelOffset + y, color);
            }
        }

        part = entity;
        if (canvas != null)
        {
            canvas.sortingOrder = 1000 - (int)entity.GetComponentObject<EntityAuthoring>().transform.position.z;
        }
        texture.Apply();

        bool flickering = false;

        foreach (var effect in entity.GetEffects())
        {
            if (IconDataBase.GetEffectIcon(effect.EffectName) != null && effect.duration != -1)
            {
                flickering = true;
                break;
            }
        }
        partImage.color = Color.white;
        if (flickering)
        {
            _flicker.Activate();
        }
        else
        {
            _flicker.Deactivate();
        }
    }

    protected override void Awake()
    {
        partImage = GetComponent<Image>();
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        targetGraphic = partImage;
        _parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
        _outline = GetComponent<Outline>();
        _flicker = GetComponent<UiFlicker>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnPointerEnterDelegate?.Invoke(this);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitDelegate?.Invoke(this);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnPointerDownDelegate?.Invoke(this);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        OnPointerUpDelegate?.Invoke(this);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        OnSelectDelegate?.Invoke(this);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        OnDeselectDelegate?.Invoke(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragDelegate?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDropDelegate?.Invoke(this);
    }
}