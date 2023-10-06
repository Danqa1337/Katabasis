using System;
using System.CodeDom;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

public abstract partial class MySystemBase : SystemBase
{
    protected bool _debug = false;
    private static ManualCommanBufferSytem _manualCommanBufferSytem;
    protected List<string> _debugMessages = new List<string>();
    private List<Action> _scheduledEvents = new List<Action>();

    protected override void OnCreate()
    {
        base.OnCreate();
        _manualCommanBufferSytem = ManualCommanBufferSytem.Instance;
    }

    protected override void OnDestroy()
    {
    }

    protected Unity.Mathematics.Random GetRandom()
    {
        return new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
    }

    protected EntityCommandBuffer CreateEntityCommandBuffer()
    {
        return _manualCommanBufferSytem.CreateCommandBuffer();
    }

    protected EntityCommandBuffer.ParallelWriter CreateEntityCommandBufferParallel()
    {
        return _manualCommanBufferSytem.CreateCommandBuffer().AsParallelWriter();
    }

    protected void UpdateECB()
    {
        Dependency.Complete();
        _manualCommanBufferSytem.Update();
    }

    protected void WriteDebug()
    {
        if (_debug)
        {
            Debug.Log(GetType().ToString() + " made " + _debugMessages.Count + " operations " + string.Concat(_debugMessages));
            _debugMessages.Clear();
        }
    }

    protected void WriteDebug(UnsafeParallelHashSet<FixedString512Bytes> messages)
    {
        if (_debug)
        {
            Debug.Log(GetType().ToString() + " made " + messages.Count() + " operations " + string.Concat(messages));
            messages.Dispose();
        }
    }

    protected void NewDebugMessage(string message)
    {
        if (_debug)
        {
            _debugMessages.Add("\n" + message);
        }
    }

    public static void NewDebugMessageParallel(string message, UnsafeParallelHashSet<FixedString32Bytes> messages)
    {
        messages.Add("\n" + message);
    }

    protected void AddToLastDebugMessage(string message)
    {
        if (_debug && _debugMessages.Count > 0)
        {
            _debugMessages[_debugMessages.Count - 1] += message;
        }
    }

    protected void ScheduleTriggerEvent(Action action)
    {
        _scheduledEvents.Add(action);
    }

    protected void TriggerEvents()
    {
        foreach (var item in _scheduledEvents)
        {
            item?.Invoke();
        }
        _scheduledEvents.Clear();
    }
}

public static class ManualSystemUpdater
{
    public static void Update<T>() where T : MySystemBase
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<T>().Update();
    }
}