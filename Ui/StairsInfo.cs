using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locations;

using System.Linq;
public class StairsInfo : Singleton<StairsInfo>
{
    private static List<TransitionIcon> _activeIcons = new List<TransitionIcon>();
    private static List<TransitionIcon> _disabledIcons = new List<TransitionIcon>();
    [SerializeField] private Transform _disabledHolder;
    public static void Clear()
    {
        foreach (var item in _activeIcons)
        {
            item.transform.SetParent(instance._disabledHolder);
            item.StopFlickering();
            _disabledIcons.Add(item);
        }
        _activeIcons.Clear();
    }
    private void Awake()
    {
        _disabledIcons = instance._disabledHolder.GetComponentsInChildren<TransitionIcon>().ToList();
        _activeIcons = new List<TransitionIcon>();
    }
    public static void UpdateInfo()
    {
        Clear();
        foreach (var transitionId in Registers.GlobalMapRegister.CurrentLocation.TransitionsIDs)
        {
            Debug.Log("Stairs info updated");
            var transition = Registers.GlobalMapRegister.GetTransition(transitionId);
            
            if(transition.ExposedEndTile.maped)
            {
                DrawNewIcon(transition);
            }
        }
    }
    public static void DrawNewIcon(Transition transition)
    {
        SoundSystem.PlaySound(SoundName.FindStairs);
        var transitionIcon = _disabledIcons[0];
        _disabledIcons.Remove(transitionIcon);
        _activeIcons.Add(transitionIcon);
        transitionIcon.transform.SetParent(instance.transform);
        transitionIcon.transform.position = Vector3.zero;
        transitionIcon.DrawTransition(transition);
    }
}
