using Gods;
using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[Serializable]
public class GodsRegister : IRegisterWithSubscription
{
    private readonly SerializableDictionary<int, God> _godsByIndexes = new SerializableDictionary<int, God>();

    public static event Action<God> OnRelationsChanged;

    public static event Action<God> OnAttentionChanged;

    public God[] GetAllGods()
    {
        return _godsByIndexes.Values.Where(g => g != null).ToArray();
    }

    public GodsRegister(God[] gods)
    {
        Debug.Log(gods.Length + " gods found");
        foreach (var god in gods)
        {
            god.SetRelations((int)UnityEngine.Random.Range(0, 100));
            god.SetAttention((int)UnityEngine.Random.Range(0, 30));
            god.OnAttentionChanged += delegate { OnAttentionChanged?.Invoke(god); };
            god.OnRelationsChanged += delegate { OnRelationsChanged?.Invoke(god); };
            _godsByIndexes.Add(god.Index, god);
        }
    }

    public void OnEnable()
    {
        TimeController.OnTickStarted += OnTick;
    }

    public void OnDisable()
    {
        TimeController.OnTickStarted -= OnTick;
    }

    private void OnTick()
    {
        foreach (var item in GetAllGods())
        {
            item.OnTick();
        }
    }

    public void Pray()
    {
        foreach (var item in GetAllGods())
        {
            item.OnPray();
        }
    }

    public void OnSacrifice(SacrificeData sacrificeData)
    {
        foreach (var item in GetAllGods())
        {
            item.OnSacrifice(sacrificeData);
        }
    }

    public God GetGod(GodArchetype GodArchetype)
    {
        return _godsByIndexes.Values.FirstOrDefault(g => g.GodArchetype == GodArchetype);
    }

    public God GetGod(int index)
    {
        return _godsByIndexes[index];
    }

    public bool GodExists(int index)
    {
        return _godsByIndexes.ContainsKey(index);
    }
}