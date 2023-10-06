using Gods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class GodsMinimap : MonoBehaviour
{
    [SerializeField] private GodMiniIcon _godIconPrefab;
    [SerializeField] private bool _debug;
    [SerializeField] private Astrolabia _astrolabia;
    [SerializeField] private ImageBlinkEffect _imageBlinkEffect;
    private bool _started;

    private void OnEnable()
    {
        GodsRegister.OnRelationsChanged += ListenGods;
        GodsRegister.OnAttentionChanged += ListenGods;
    }

    private void OnDisable()
    {
        GodsRegister.OnRelationsChanged -= ListenGods;
        GodsRegister.OnAttentionChanged -= ListenGods;
    }

    private void Start()
    {
        StartUp();
    }

    private void ListenGods(God god)
    {
        Redraw();
    }

    private void StartUp()
    {
        _started = true;
        foreach (var god in Registers.GodsRegister.GetAllGods())
        {
            var icon = Instantiate(_godIconPrefab, _astrolabia.transform);
            _astrolabia.AttachNew(icon.transform);
            icon.Draw(god);
        }
        Redraw();
        _astrolabia.OnMovingTransforms += IndicateAstrolabiaChange;
    }

    private void IndicateAstrolabiaChange()
    {
        _imageBlinkEffect.Play();
    }

    private void Redraw()
    {
        if (_started)
        {
            var positions = new Dictionary<int, int>();
            var gods = Registers.GodsRegister.GetAllGods();
            for (int i = 0; i < gods.Length; i++)
            {
                positions.Add(i, (int)gods[i].AttentionLevel);
            }
            _astrolabia.Redraw(positions);
        }
        else
        {
            StartUp();
        }
    }
}