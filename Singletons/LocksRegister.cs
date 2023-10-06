using System.Collections.Generic;
using Unity.Entities;

public class LocksRegister
{
    private SerializableDictionary<int, Lock> _locks;

    public LocksRegister()
    {
        _locks = new SerializableDictionary<int, Lock>();
    }

    public int RegisterNewLock()
    {
        int index = 0;
        do
        {
            index = UnityEngine.Random.Range(2, int.MaxValue);
        }
        while (_locks.ContainsKey(index));

        _locks.Add(index, new Lock());
        return index;
    }

    public bool IsLockExists(int index)
    {
        return _locks.ContainsKey(index);
    }

    public Lock GetLock(int index)
    {
        if (IsLockExists(index))
        {
            return _locks[index];
        }
        else
        {
            return null;
        }
    }

    public void UnLock(int index)
    {
        GetLock(index).locked = false;
    }

    [System.Serializable]
    public class Lock
    {
        public bool locked = true;
    }
}

[System.Serializable]
public struct LockComponent : IComponentData
{
    public int lockIndex;

    public LockComponent(int lockIndex)
    {
        this.lockIndex = lockIndex;
    }
}

[System.Serializable]
public struct KeyComponent : IComponentData
{
    public int lockIndex;

    public KeyComponent(int lockIndex)
    {
        this.lockIndex = lockIndex;
    }
}