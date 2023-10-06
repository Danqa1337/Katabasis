using UnityEngine;

public class BlankUICanvas : UiCanvas
{
    [SerializeField] private UIName _uIName;
    public override UIName UIName => _uIName;
}