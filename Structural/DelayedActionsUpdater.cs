using System;
using System.Collections.Generic;

public static class DelayedActionsUpdaterOnTick
{
    private static List<(int, Action)> _delayedActions = new List<(int, Action)>();


    public static void ScheduleAction(int delay, Action action)
    {
        _delayedActions.Add((delay, action));
    }
    public static void CancelAllActions()
    {
        _delayedActions.Clear();
    }

    public static void Update()
    {
        for (int i = 0; i < _delayedActions.Count; i++)
        {
            _delayedActions[i] = (_delayedActions[i].Item1 - 1, _delayedActions[i].Item2);
            if (_delayedActions[i].Item1 <= 0)
            {
                _delayedActions[i].Item2.Invoke();
                _delayedActions.Remove(_delayedActions[i]);
            }
        }

    }
}
public static class DelayedActionsUpdaterOnCycle
{
    public static bool Empty => _delayedActions.Count == 0;

    private static List<Action> _delayedActions = new List<Action>();

    public static List<Action> GetActions()
    {
        return _delayedActions;
    }
    public static void ScheduleAction(Action action)
    {
        _delayedActions.Add((action));
    }
    public static void CancelAllActions()
    {
        _delayedActions.Clear();
    }

    public static void Update()
    {
        foreach (var item in _delayedActions)
        {
            item.Invoke();
        }
        _delayedActions.Clear();

    }
}
