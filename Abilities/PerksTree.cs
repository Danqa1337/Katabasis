using Perks;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public static class PerksTree
{
    public static event Action<PerkName, Entity> OnPerkGranted;

    public static event Action<PerkName, Entity> OnPerkRevoked;

    public static bool HasPerk(Entity entity, PerkName perkName)
    {
        return entity.GetBuffer<PerkElement>().Contains(new PerkElement(perkName));
    }

    public static void GrantPerk(PerkName perkName, Entity entity)
    {
        if (HasPerk(entity, perkName))
        {
            Debug.Log(perkName + " is allready active");
        }
        else
        {
            PerksDatabase.GetPerk(perkName).Grant(entity);
            Debug.Log(perkName + " activated");
            OnPerkGranted?.Invoke(perkName, entity);
        }
    }

    public static void RevokePerk(PerkName perkName, Entity entity)
    {
        if (!HasPerk(entity, perkName))
        {
            Debug.Log(perkName + " is allready inactive");
        }
        else
        {
            PerksDatabase.GetPerk(perkName).Revoke(entity);
            Debug.Log(perkName + " deactivated");
            OnPerkRevoked?.Invoke(perkName, entity);
        }
    }
}

[Serializable]
public struct PerkElement : IBufferElementData
{
    public readonly PerkName PerkName;

    public PerkElement(PerkName perkName)
    {
        PerkName = perkName;
    }
}