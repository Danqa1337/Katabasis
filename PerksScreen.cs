using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PerksScreen : MonoBehaviour
{
    [SerializeField] private PerkIcon _perkIconPrefab;
    [SerializeField] private Transform _layout;

    private void OnEnable()
    {
        PerksTree.OnPerkGranted += ListenPerks;
        PerksTree.OnPerkRevoked += ListenPerks;
    }

    private void OnDisable()
    {
        PerksTree.OnPerkGranted -= ListenPerks;
        PerksTree.OnPerkRevoked -= ListenPerks;
    }

    private void Start()
    {
        Clear();
    }

    private void ListenPerks(PerkName perkName, Entity entity)
    {
        if (entity.IsPlayer())
        {
            Redraw();
        }
    }

    public void Redraw()
    {
        Clear();
        foreach (var perkElement in Player.PlayerEntity.GetBuffer<PerkElement>())
        {
            var perkIcon = Instantiate(_perkIconPrefab, Vector3.zero, Quaternion.identity);
            perkIcon.transform.SetParent(_layout.transform);
            perkIcon.DrawPerk(perkElement.PerkName);
        }
    }

    private void Clear()
    {
        foreach (var item in _layout.transform.GetChildren())
        {
            Destroy(item);
        }
    }
}