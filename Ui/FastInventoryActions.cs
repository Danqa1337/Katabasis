using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine.InputSystem;

[Binding]
public class FastInventoryActions : Singleton<FastInventoryActions>
{
    private static Canvas _canvas;
    private bool _canEat;
    private bool _canDrop;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private KatabasisButton _eatButton;
    [SerializeField] private KatabasisButton _dropButton;
    [SerializeField] private KatabasisButton _useButton;
    private IItemDonor _selectedSlot;
    private static bool _active => _canvas.enabled;

    private void OnEnable()
    {
        Coursor.OnLeftClick += Hide;
    }

    private void OnDisable()
    {
        Coursor.OnLeftClick -= Hide;
    }

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        Hide();
    }

    public void ListenInput()
    {
    }

    public static void Show(IItemDonor slot)
    {
        if (slot.Item != Entity.Null)
        {
            instance._selectedSlot = slot;
            _canvas.enabled = true;
            instance.transform.position = Mouse.current.position.ReadValue() + instance._offset;
            instance._eatButton.gameObject.SetActive(Player.IsEatableItem(slot.Item));
        }
    }

    public static void Hide()
    {
        instance.StartCoroutine(hide());
        IEnumerator hide()
        {
            yield return new WaitForSeconds(0.2f);
            _canvas.enabled = false;
            instance.Clear();
        }
    }

    private void Clear()
    {
        _eatButton.ClearEvents();
        _dropButton.ClearEvents();
        _useButton.ClearEvents();
    }

    public void Drop()
    {
        Debug.Log("drop");
        ItemsOnTheGround.DropItem(_selectedSlot.TakeItem());
    }

    public void Eat()
    {
        UiManager.HideUiCanvas(UIName.Inventory);
        Player.Eat(_selectedSlot.Item);
    }

    public void Use()
    {
        UiManager.ShowUiCanvas(UIName.Aiming);
    }
}