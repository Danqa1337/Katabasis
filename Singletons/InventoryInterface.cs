using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityWeld.Binding;
using System.ComponentModel;
using System;
using System.Text;
using System.Collections;

[Binding]
public abstract class InventoryInterface : MonoBehaviour, IDropHandler, IItemReciever
{
    public event PropertyChangedEventHandler PropertyChanged;

    private List<ItemStack> _stacks = new List<ItemStack>();
    [SerializeField] private LayoutGroup _layout;
    [SerializeField] private bool _debug;
    private string _inventoryName;

    [Binding]
    public string InventoryName
    {
        get
        {
            return _inventoryName;
        }
        set
        {
            _inventoryName = value;
            InvokePropertyChange("InventoryName");
        }
    }

    protected void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }

    public ItemStack FindSackWithItem(Entity entity)
    {
        var itemName = entity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName;
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].ItemName == itemName)
            {
                return _stacks[i];
            }
        }
        return null;
    }

    protected virtual void Redraw(Inventory inventory)
    {
        if (_debug) Debug.Log(String.Format("Redrowing {0} . It has {1} items", inventory.Name, inventory.Items.Length.ToString()));
        InventoryName = inventory.Name;
        var expandedStacks = new List<SimpleObjectName>();
        foreach (var item in _stacks)
        {
            if (item.IsExpanded)
            {
                expandedStacks.Add(item.ItemName);
            }
            item.Collapse();
            item.Clear();
            Pooler.Put(item.GetComponent<PoolableItem>());
        }
        _stacks.Clear();

        foreach (var item in inventory.Items.OrderBy(item => item.GetComponentData<SimpleObjectNameComponent>().simpleObjectName))
        {
            var itemName = item.GetComponentData<SimpleObjectNameComponent>().simpleObjectName;
            var stack = _stacks.FirstOrDefault(s => s.ItemName == itemName);
            if (stack == null)
            {
                stack = _stacks.FirstOrDefault(s => s.IsEmpty);
            }

            if (stack == null)
            {
                stack = CreateNewStack();
            }

            stack.Add(item);
        }

        for (int i = 0; i < _stacks.Count; i++)
        {
            var stack = _stacks[i];

            if (stack.IsEmpty)
            {
                Pooler.Put(stack.GetComponent<PoolableItem>());
                _stacks.Remove(stack);
            }
            else
            {
                stack.Redraw();

                if (expandedStacks.Contains(stack.ItemName))
                {
                    stack.Expand();
                }
            }
        }
        ForceRebuildLayout();
    }

    public void ForceRebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layout.transform as RectTransform);
    }

    private ItemStack CreateNewStack()
    {
        var newStack = Pooler.Take("ItemStack", Vector3.zero).GetComponent<ItemStack>();
        newStack.transform.SetParent(_layout.transform);
        newStack.OnItemGiven += Remove;
        newStack.OnItemRecieved += RecieveItem;
        newStack.OnDubbleClickOnItem += OnDubbleClickOnItem;
        newStack.OnRebuildLayout += ForceRebuildLayout;
        _stacks.Add(newStack);

        return newStack;
    }

    protected abstract void PlaceItem(Entity entity);

    public abstract void Remove(Entity entity);

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Ondrop");
        RecieveItem(Coursor.EndDrag());
    }

    public void RecieveItem(IItemDonor donor)
    {
        if (donor != null)
        {
            Debug.Log("OnRecieve");
            var item = donor.TakeItem();
            PlaceItem(item);
        }
    }

    protected abstract void OnDubbleClickOnItem(IItemDonor itemDonor);
}