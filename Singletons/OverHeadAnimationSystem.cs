using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;
public enum OverHeadAnimationType
{
    Dialog,
    Fear,
    Stun,
}

[DisableAutoCreation]
public partial class OverHeadAnimationSystem : MySystemBase
{
    protected override void OnUpdate()
    {

        var ecb = CreateEntityCommandBuffer();
        Entities.ForEach((Entity entity, Transform transform, ref DynamicBuffer<OverHeadAnimationElement> animationElements,  ref DynamicBuffer<ChangeOverHeadAnimationElement> changeAnimationElements) =>
        {
            ecb.RemoveComponent<ChangeOverHeadAnimationElement>(entity);
            ecb.AddComponent(entity, new OverHeadAnimationBufferChanged());
            foreach (var changeElement in changeAnimationElements)
            {
                var newAnimation = new OverHeadAnimationElement(changeElement.amimationType);
                if (changeElement.add)
                {
                    if (!animationElements.Contains(newAnimation))
                    {
                        ecb.AddBufferElement(entity, newAnimation);
                        if (!entity.HasComponent<OverHeadAnimator>() && transform.GetComponentInChildren<OverHeadAnimator>() == null)
                        {

                            var overHeadAnimator = Pooler.Take("OverHeadAnimation", Vector3.zero).GetComponent<OverHeadAnimator>();
                            ecb.AddComponent(entity, overHeadAnimator);
                            overHeadAnimator.renderer.enabled = true;
                            overHeadAnimator.transform.SetParent(transform);
                            overHeadAnimator.transform.localPosition = new Vector3(0, 1, 0);
                        }
                    }
                }
                else
                {

                    if (animationElements.Contains(newAnimation))
                    {
                        animationElements.Remove(newAnimation);
                        
                    }
                    
                    
                }
            }

        }).WithoutBurst().Run();
        UpdateECB();
        ecb = CreateEntityCommandBuffer();
        Entities.WithAll<OverHeadAnimationBufferChanged>().ForEach((Entity entity, OverHeadAnimator animator, in DynamicBuffer<OverHeadAnimationElement> animationElements) =>
        {

            ecb.RemoveComponent<OverHeadAnimationBufferChanged>(entity);
            if (animationElements.Length > 0)
            {
                var strongestElement = animationElements[0].amimationType;
                for (int i = 0; i < animationElements.Length; i++)
                {
                    if ((int)animationElements[i].amimationType > (int)strongestElement)
                    {
                        strongestElement = animationElements[i].amimationType;
                    }
                }
                animator.Play(strongestElement);
            }
            else
            {
                ecb.AddComponent<ClearOverHeadAnimations>(entity);
            }

        }).WithoutBurst().Run();
        UpdateECB();

        ecb = CreateEntityCommandBuffer();
        Entities.WithAll<ClearOverHeadAnimations>().ForEach((Entity entity, OverHeadAnimator animator, ref DynamicBuffer<OverHeadAnimationElement> animationElements) =>
        {
            ecb.RemoveComponent<OverHeadAnimator>(entity);
            ecb.RemoveComponent<ClearOverHeadAnimations>(entity);
            animationElements.Clear();
            Pooler.Put(animator.GetComponent<PoolableItem>());

        }).WithoutBurst().Run();
        UpdateECB();
        
        

    }
    private struct OverHeadAnimationBufferChanged : IComponentData
    {

    }
}
public struct ChangeOverHeadAnimationElement : IBufferElementData
{
    public bool add;
    public OverHeadAnimationType amimationType;

    public ChangeOverHeadAnimationElement(OverHeadAnimationType amimationType, bool add)
    {
        this.add = add;
        this.amimationType = amimationType;
    }
}
public struct ClearOverHeadAnimations : IComponentData
{

}
public struct OverHeadAnimationElement : IBufferElementData
{
    public OverHeadAnimationType amimationType;

    public OverHeadAnimationElement(OverHeadAnimationType amimationType)
    {
        this.amimationType = amimationType;
    }
}
