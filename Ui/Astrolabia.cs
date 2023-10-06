using Gods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Pseudo;
using UnityEngine.UI;

public class Astrolabia : MonoBehaviour
{
    [SerializeField] private RadialLayout[] _layouts;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private Transform _stars;
    [SerializeField] private Transform _center;
    private bool _rotationEnabled;
    private bool _rebuilding;
    private List<int> _layoutIndices = new List<int>();
    private List<Transform> _transforms = new List<Transform>();

    public event Action OnMovingTransforms;

    private void Update()
    {
        if (_rotationEnabled)
        {
            var rotation = _rotationSpeed * Time.deltaTime;
            foreach (var layout in _layouts)
            {
                layout.StartAngle += rotation;
                layout.CalculateRadial();
            }
            _stars.transform.Rotate(-new Vector3(0, 0, rotation));
        }
    }

    public void AttachNew(Transform item)
    {
        var layoutNum = _layouts.Length - 1;
        item.SetParent(_layouts[layoutNum].transform);
        _transforms.Add(item);
        _layoutIndices.Add(layoutNum);
    }

    public void Redraw(Dictionary<int, int> newPositions)
    {
        StartCoroutine(RedrawIE(newPositions));
    }

    public IEnumerator RedrawIE(Dictionary<int, int> newlayoutIndexes)
    {
        while (_rebuilding)
        {
            yield return new WaitForEndOfFrame();
        }

        if (_layoutIndices.Count > 0)
        {
            _rebuilding = true;
            var transformsToMove = new Dictionary<int, Transform>();
            var targetLayoutIndices = new Dictionary<int, int>();
            var oldLayoutIndices = new Dictionary<int, int>();

            for (int index = 0; index < _layoutIndices.Count; index++)
            {
                if (newlayoutIndexes[index] != _layoutIndices[index])
                {
                    var transform = _transforms[index];
                    oldLayoutIndices.Add(index, _layoutIndices[index]);
                    targetLayoutIndices.Add(index, newlayoutIndexes[index]);
                    transformsToMove.Add(index, transform);
                }
            }

            if (transformsToMove.Count > 0)
            {
                OnMovingTransforms?.Invoke();
                StartCoroutine(MoveTransforms(transformsToMove, oldLayoutIndices, targetLayoutIndices));
            }
            else
            {
                _rebuilding = false;
            }
        }
    }

    private IEnumerator MoveTransforms(Dictionary<int, Transform> transforms, Dictionary<int, int> oldLayoutIndices, Dictionary<int, int> targetLayoutIndices)
    {
        Debug.Log("moving " + transforms.Count);
        var dummies = new Dictionary<int, Transform>();
        var oldPositions = new Dictionary<int, Vector3>();
        var frameCount = 25f;

        PauseRotation();

        foreach (var index in transforms.Keys)
        {
            transforms[index].SetParent(_center);
            dummies.Add(index, SpawnDummy(targetLayoutIndices[index]));
        }
        RebuildLayouts();

        foreach (var index in transforms.Keys)
        {
            oldPositions.Add(index, _center.position + (dummies[index].transform.position - _center.position).normalized * _layouts[oldLayoutIndices[index]].fDistance);
        }

        for (float t = 0; t < frameCount; t++)
        {
            foreach (var index in transforms.Keys)
            {
                transforms[index].transform.position = Vector3.Lerp(oldPositions[index], dummies[index].transform.position, t / frameCount);
            }
            yield return new WaitForSeconds(1 / frameCount);
        }

        foreach (var index in transforms.Keys)
        {
            Destroy(dummies[index].gameObject);
            _layoutIndices[index] = targetLayoutIndices[index];
            transforms[index].SetParent(_layouts[targetLayoutIndices[index]].transform);
        }

        RebuildLayouts();
        ContinueRotation();
        _rebuilding = false;
    }

    private Transform SpawnDummy(int position)
    {
        var layout = _layouts[position];
        var dummy = new GameObject("dummy");
        dummy.AddComponent<RectTransform>();
        dummy.transform.SetParent(layout.transform);
        layout.CalculateRadial();
        return dummy.transform;
    }

    private void RebuildLayouts()
    {
        foreach (var item in _layouts)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.transform as RectTransform);
        }
    }

    public void PauseRotation()
    {
        _rotationEnabled = false;
    }

    public void ContinueRotation()
    {
        _rotationEnabled = true;
    }

    private void Clear()
    {
        foreach (var item in GetComponentsInChildren<GodMiniIcon>())
        {
            Destroy(item.gameObject);
        }
    }
}