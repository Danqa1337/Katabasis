using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Locations;

public class TransitionIcon : KatabasisSelectable
{
    [SerializeField] private Sprite _downSprite;
    [SerializeField] private Sprite _upSprite;
    private Image _image;
    private int _transitionId;
    private int2 transitionPosition;
    private ParticleSystem _particleSystem;
    private UiFlicker _uiFlicker;

    public static event Action<Locations.Transition> OnPointerEnterEvent;

    public static event Action OnPointerExitEvent;

    protected override void Awake()
    {
        _image = GetComponent<Image>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _uiFlicker = GetComponent<UiFlicker>();
    }

    public void DrawTransition(Locations.Transition transition)
    {
        Flick();
        _transitionId = transition.id;
        var currentLocation = Registers.GlobalMapRegister.CurrentLocation;

        if (transition.entranceLocationId == currentLocation.Id)
        {
            _image.sprite = _downSprite;
            transitionPosition = transition.entrancePosition;
        }
        else if (transition.exitLoctionId == currentLocation.Id)
        {
            _image.sprite = _upSprite;
            transitionPosition = transition.exitPosition;
        }
        else
        {
            throw new System.ArgumentOutOfRangeException();
        }
    }

    [ContextMenu("Flick")]
    public void Flick()
    {
        StartCoroutine(DoFlick());
        IEnumerator DoFlick()
        {
            StopFlickering();
            _uiFlicker.Activate();
            yield return new WaitForSeconds(2);
            _uiFlicker.Deactivate();
        }
    }

    public void StopFlickering()
    {
        _uiFlicker.Deactivate();
    }

    protected override void OnLeftClick()
    {
        MainCameraHandler.instance.transform.position = transitionPosition.ToVector3();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnPointerEnterEvent?.Invoke(Registers.GlobalMapRegister.GetTransition(_transitionId));
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitEvent?.Invoke();
    }
}