using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class GodsScreen : MonoBehaviour
{
    [SerializeField] private GodIcon _godIconPrefab;
    [SerializeField] private bool _debug;
    [SerializeField] private Astrolabia _astrolabia;

    private void OnEnable()
    {
        UiManager.OnShowCanvas += OnShowCanvas;
    }

    private void OnDisable()
    {
        UiManager.OnShowCanvas -= OnShowCanvas;
    }

    private void Start()
    {
        StartUp();
    }

    private void OnShowCanvas(UIName uIName)
    {
        if (uIName == UIName.Gods)
        {
            Redraw();
        }
    }

    private void StartUp()
    {
        foreach (var god in Registers.GodsRegister.GetAllGods())
        {
            Debug.Log(Registers.GodsRegister.GetAllGods() + " gods");
            var icon = Instantiate(_godIconPrefab, _astrolabia.transform);
            _astrolabia.AttachNew(icon.transform);
            icon.Draw(god);
        }
        Redraw();
    }

    private void Redraw()
    {
        var positions = new Dictionary<int, int>();
        var gods = Registers.GodsRegister.GetAllGods();
        for (int i = 0; i < gods.Length; i++)
        {
            positions.Add(i, (int)gods[i].AttentionLevel);
        }
        _astrolabia.Redraw(positions);
    }
}