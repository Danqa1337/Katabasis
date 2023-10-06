using Unity.Entities;
using UnityEngine;

public class OverHeadAnimator : MonoBehaviour, IComponentData
{
    private Animator _animator;
    [HideInInspector]public SpriteRenderer renderer;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
    }

    public void Play(OverHeadAnimationType hoveringAmimationType)
    {
        _animator.Play(hoveringAmimationType.ToString());
    }
}