using System.Collections.Generic;
using Locations;
using System;

[System.Serializable]
public class GlobalMapRegister : IRegisterWithSubscription
{
    public List<Location> _locations = new List<Location>();
    public List<Transition> _transitions = new List<Transition>();

    private Location _pit;
    private Location _arena;

    public int _depth;
    private int _currentLocationId;

    public GlobalMapRegister(List<Location> locations, List<Transition> transitions, Location pit, Location arena, int depth)
    {
        _locations = locations;
        _transitions = transitions;
        _pit = pit;
        _arena = arena;
        _depth = depth;
    }

    public int CurrentLocationId => _currentLocationId;
    public Location CurrentLocation => GetLocation(_currentLocationId);
    public int depth => _depth;
    public List<Location> Locations => _locations;
    public List<Transition> Transitions => _transitions;

    public Location Pit { get => _pit; }
    public Location Arena { get => _arena; }

    public void OnEnable()
    {
        LocationGenerator.OnLocationGenerationComplete += SetLocationAsCurrent;
        LocationLoader.OnLocationLoaded += SetLocationAsCurrent;
    }

    public void OnDisable()
    {
        LocationGenerator.OnLocationGenerationComplete -= SetLocationAsCurrent;
        LocationLoader.OnLocationLoaded -= SetLocationAsCurrent;
    }

    private void SetLocationAsCurrent(Location location)
    {
        _currentLocationId = location.Id;
    }

    public Transition GetTransition(int id)
    {
        foreach (var transition in _transitions)
        {
            if (transition.id == id) return transition;
        }
        throw new NullReferenceException("no such transition with id " + id);
    }

    public Location GetLocation(int id)
    {
        foreach (var location in _locations)
        {
            if (location.Id == id) return location;
        }

        throw new NullReferenceException("no such location");
    }
}