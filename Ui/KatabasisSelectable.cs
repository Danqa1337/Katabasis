using FMOD;
using Unity.Entities;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System.ComponentModel;
using UnityEngine.Events;

public abstract class KatabasisSelectable : Selectable, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public UnityEvent OnLeftClickEvent;
    public UnityEvent OnRightClickEvent;
    public UnityEvent OnDubbleClickEvent;

    private float _timeSinceLastPointerDown;
    private float _timeSinceLastClick;
    private const float MaxClickWindow = 0.3f;

    protected virtual void Update()
    {
        _timeSinceLastPointerDown += Time.deltaTime;
        _timeSinceLastClick += Time.deltaTime;
    }

    public void ClearEvents()
    {
        OnRightClickEvent.RemoveAllListeners();
        OnLeftClickEvent.RemoveAllListeners();
        OnDubbleClickEvent.RemoveAllListeners();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        _timeSinceLastPointerDown = 0;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (_timeSinceLastPointerDown < MaxClickWindow)
        {
            RegisterClick(eventData);
        }
    }

    private void RegisterClick(PointerEventData eventData)
    {
        if (_timeSinceLastClick < MaxClickWindow && eventData.button == PointerEventData.InputButton.Left)
        {
            OnDubbleClick();
        }
        else
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick();
            }
        }
        _timeSinceLastClick = 0;
    }

    protected virtual void OnLeftClick()
    {
        OnLeftClickEvent?.Invoke();
    }

    protected virtual void OnDubbleClick()
    {
        OnDubbleClickEvent?.Invoke();
    }

    protected virtual void OnRightClick()
    {
        OnRightClickEvent?.Invoke();
    }

    protected void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }
}