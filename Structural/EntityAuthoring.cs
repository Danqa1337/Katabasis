using System;
using UnityEngine;

public class EntityAuthoring : MonoBehaviour
{
    public Transform partsHolder;
    public RendererComponent bodyRenderer;
    public Action OnDeath;

    public async void Destroy()
    {
        OnDeath?.Invoke();
        await bodyRenderer.Desolve();

        var children = partsHolder.transform.GetComponentsInChildren<EntityAuthoring>(true);
        foreach (var child in children)
        {
            child.transform.SetParent(null);
            Pooler.Put(child.gameObject);
        }
        foreach (var item in GetComponentsInChildren<OverHeadAnimator>())
        {
            Pooler.Put(item.GetComponent<PoolableItem>());
        }
        Pooler.Put(gameObject);
    }
}