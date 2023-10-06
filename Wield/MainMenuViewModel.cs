using System.ComponentModel;
using UnityWeld.Binding;
[Binding]
public class MainMenuViewModel : Singleton<MainMenuViewModel>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private bool _canContinue;


}
