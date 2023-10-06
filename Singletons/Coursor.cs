using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Coursor : Singleton<Coursor>
{
    [SerializeField] private ImagesStack _imagesStack;
    private RectTransform _rectTransform;
    private PlayerInput _playerInput;
    private IItemDonor _itemDonorDragFrom;

    public static event Action OnLeftClick;

    public static event Action OnRightClick;

    private void OnEnable()
    {
        _playerInput.onActionTriggered += ListenInput;
    }

    private void OnDisable()
    {
        _playerInput.onActionTriggered -= ListenInput;
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        _rectTransform.anchoredPosition = Mouse.current.position.ReadValue();
    }

    private void ListenInput(InputAction.CallbackContext callbackContext)
    {
        StartCoroutine(Listen());
        IEnumerator Listen()
        {
            ControllerActionName controllerActionName = ControllerActionName.NULL;
            if (callbackContext.action.name.DecodeCharSeparatedEnumsAndGetFirst<ControllerActionName>(out controllerActionName))
            {
                if (controllerActionName == ControllerActionName.LeftClick)
                {
                    if (callbackContext.performed)
                    {
                        OnLeftClick?.Invoke();
                        Clear();
                    }
                    if (callbackContext.canceled)
                    {
                        yield return new WaitForSeconds(0.2f);
                        Clear();
                    }
                }
                if (controllerActionName == ControllerActionName.RightClick)
                {
                    OnRightClick?.Invoke();
                }
            }
        }
    }

    private void Clear()
    {
        _itemDonorDragFrom = null;
        instance._imagesStack.Clear();
    }

    public static void StartDrag(IItemDonor donor)
    {
        Debug.Log("Begin drag " + donor.Item.GetName());
        instance._itemDonorDragFrom = donor;
        instance._imagesStack.DrawItem(donor.Item);
    }

    public static IItemDonor EndDrag()
    {
        var donor = instance._itemDonorDragFrom;
        instance.Clear();
        return donor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndDrag();
    }
}