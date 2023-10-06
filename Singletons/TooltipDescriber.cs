using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class TooltipDescriber : MonoBehaviour, INotifyPropertyChanged
{
    private TooltipCanvas _tooltipCanvas;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }

    private void Awake()
    {
        _tooltipCanvas = GetComponent<TooltipCanvas>();
    }

    protected void Show()
    {
        _tooltipCanvas.Show();
    }

    protected void Hide()
    {
        _tooltipCanvas.Hide();
    }
}