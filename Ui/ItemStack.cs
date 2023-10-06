using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityWeld.Binding;

[Binding]
public class ItemStack : KatabasisSelectable, IItemDonor, IItemReciever, IBeginDragHandler, IDropHandler
{
    public bool IsEmpty => _items.Count == 0;
    public bool IsExpanded => _isExpanded;
    private List<Entity> _items = new List<Entity>();
    private bool _isExpanded;
    private string _amount;
    private string _name;
    private List<InventorySlot> _slots = new List<InventorySlot>();
    [SerializeField] private LayoutGroup _layout;
    [SerializeField] private ImagesStack _imagesStack;

    public event Action<Entity> OnItemGiven;

    public event Action<IItemDonor> OnItemRecieved;

    public event Action OnRebuildLayout;

    public event Action<IItemDonor> OnDubbleClickOnItem;

    public List<Entity> Items => _items;

    public SimpleObjectName ItemName
    {
        get;
        private set;
    }

    [Binding]
    public string Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            InvokePropertyChange("Amount");
        }
    }

    [Binding]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            InvokePropertyChange("Name");
        }
    }

    public Entity Item => Items[0];

    protected override void OnLeftClick()
    {
        base.OnLeftClick();
        if (_isExpanded)
        {
            Collapse();
        }
        else
        {
            Expand();
        }
    }

    public void Expand()
    {
        _isExpanded = true;
        ClearLayout();

        foreach (var item in _items)
        {
            if (item == Entity.Null) throw new NullReferenceException();
            var slot = Pooler.Take("InventorySlot", Vector3.zero).GetComponent<InventorySlot>();
            slot.transform.SetParent(_layout.transform);
            slot.OnItemGiven += OnItemGiven;
            slot.OnItemRecieved += OnItemRecieved;
            slot.OnDubbleClickEvent.AddListener(delegate { OnDubbleClickOnItem?.Invoke(slot); });
            slot.DrawItem(item);
            _slots.Add(slot);
        }
        ForceRebuildLayout();
    }

    public void Collapse()
    {
        _isExpanded = false;
        ClearLayout();
        ForceRebuildLayout();
    }

    private void ForceRebuildLayout()
    {
        OnRebuildLayout?.Invoke();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layout.transform as RectTransform);
        OnRebuildLayout?.Invoke();
    }

    private void ClearLayout()
    {
        foreach (var item in _slots)
        {
            item.Clear();
        }
        _slots.Clear();
        foreach (var item in _layout.GetComponentsInChildren<PoolableItem>())
        {
            Pooler.Put(item);
        }
    }

    public void Redraw()
    {
        if (!IsEmpty)
        {
            var mainItem = _items[0];
            _imagesStack.DrawItem(mainItem);

            if (_items.Count == 1)
            {
                Amount = "";
            }
            else
            {
                Amount = _items.Count.ToString();
            }

            var realName = DescriberUtility.GetName(mainItem);
            Name = realName;
        }
    }

    public void Clear()
    {
        OnItemRecieved = null;
        OnItemGiven = null;
        OnDubbleClickOnItem = null;
        OnRebuildLayout = null;
        _items.Clear();
        ItemName = SimpleObjectName.Null;
    }

    public void Add(Entity entity)
    {
        var itemName = entity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName;

        if (IsEmpty)
        {
            ItemName = itemName;
        }
        else if (ItemName != itemName)
        {
            throw new System.ArgumentOutOfRangeException();
        }
        _items.Add(entity);
    }

    public Entity TakeItem()
    {
        var item = Item;
        OnItemGiven?.Invoke(Item);
        return item;
    }

    public void RecieveItem(IItemDonor donor)
    {
        OnItemRecieved?.Invoke(donor);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Coursor.StartDrag(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        RecieveItem(Coursor.EndDrag());
    }

    protected override void OnDubbleClick()
    {
        base.OnDubbleClick();
        OnDubbleClickOnItem?.Invoke(this);
    }

    protected override void OnRightClick()
    {
        base.OnRightClick();
        if (Item != Entity.Null)
        {
            FastInventoryActions.Show(this);
        }
    }
}