using Unity.Entities;
using UnityWeld.Binding;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

[Binding]
public class SelectorDescriber : MonoBehaviour, INotifyPropertyChanged
{
    private (float, float) _morale;
    [SerializeField] private AnatomyPortrait _anatomyPortrait;
    [SerializeField] private Vector2 _offset;
    private Canvas _canvas;
    private bool _hasMorale;

    [Binding]
    public bool HasMorale
    {
        get
        {
            return _hasMorale;
        }
        set
        {
            _hasMorale = value;
            InvokePropertyChange("HasMorale");
        }
    }

    [Binding]
    public (float, float) Morale
    {
        get
        {
            return _morale;
        }
        set
        {
            _morale = value;
            InvokePropertyChange("Morale");
        }
    }

    private void OnEnable()
    {
        Selector.OnSelectionChanged += Describe;
    }

    private void OnDisable()
    {
        Selector.OnSelectionChanged -= Describe;
    }

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        Hide();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }

    private void Describe(TileData tileData)
    {
        if (tileData.valid && tileData.visible)
        {
            var entity = tileData.SolidLayer;
            if (entity != Entity.Null && !entity.IsInInvis() && entity.HasComponent<AIComponent>())
            {
                Describe(entity);
            }
            else
            {
                Hide();
            }
        }
        else
        {
            Hide();
        }
    }

    public void Describe(Entity entity)
    {
        if (entity.HasComponent<MoraleComponent>())
        {
            HasMorale = true;
            Morale = (entity.GetComponentData<MoraleComponent>().currentMorale, 100);
        }
        else
        {
            HasMorale = false;
        }

        _anatomyPortrait.UpdatePortrait(entity, true, false);

        foreach (var item in GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        }
        _canvas.enabled = true;
        transform.position = Selector.SelectedTile.position.ToVector3() + _offset;
    }

    public void Hide()
    {
        _canvas.enabled = false;
    }
}