using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

namespace Assets.Scripts.Singletons
{
    [Binding]
    public class ColorSchemeViewModel : Singleton<ColorSchemeViewModel>, INotifyPropertyChanged
    {
        [Binding]
        public Color AnatomyColorLow
        {
            get => ColorScheme.instance.AnatomyColorLow;

            set
            {
                ColorScheme.instance.AnatomyColorLow = value;
                InvokePropertyChange("AnatomyColorLow");
            }
        }

        [Binding]
        public Color AnatomyColorHigh
        {
            get => ColorScheme.instance.AnatomyColorHigh;
            set
            {
                ColorScheme.instance.AnatomyColorHigh = value;
                InvokePropertyChange("AnatomyColorHigh");
            }
        }

        [Binding]
        public Color AnatomyColorIntact
        {
            get => ColorScheme.instance.AnatomyColorIntact;
            set
            {
                ColorScheme.instance.AnatomyColorIntact = value;
                InvokePropertyChange("AnatomyColorIntact");
            }
        }

        [Binding]
        public ColorBlock AnatomyPartColorBlock
        {
            get => ColorScheme.instance.AnatomyPartColorBlock;
            set
            {
                ColorScheme.instance.AnatomyPartColorBlock = value;
                InvokePropertyChange("AnatomyPartColorBlock");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
        }

        private void Start()
        {
            ColorScheme.instance.LoadValues();
        }
    }
}