using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ItemDisplayingSelectable : KatabasisSelectable
{
    private ImagesStack _imagesStack;

    protected override void Awake()
    {
        _imagesStack = GetComponentInChildren<ImagesStack>();
    }

    protected virtual void Draw(Entity entity)
    {
        _imagesStack.DrawItem(entity);
    }

    public virtual void Clear()
    {
        _imagesStack.Clear();
    }
}