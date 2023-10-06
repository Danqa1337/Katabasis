using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

[Binding]
public class MainMenuButtons : Singleton<MainMenuButtons>, INotifyPropertyChanged
{
    public Button continueButton;
    private Canvas _canvas;
    private bool _canContinue;

    public event PropertyChangedEventHandler PropertyChanged;

    public void Awake()
    {
        _canvas = GetComponent<Canvas>();
        CanContinue = DataSaveLoader.SaveExists<PlayerSquadSaveData>();
        InvokePropertyChange("GameVersion");
    }

    [Binding]
    public string GameVersion
    {
        get => Application.version;
        set
        {
        }
    }

    [Binding]
    public bool CanContinue
    {
        get => _canContinue;
        set
        {
            _canContinue = value;
            InvokePropertyChange("CanContinue");
        }
    }

    private void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }

    [Binding]
    public void NewGame()
    {
        UiManager.ShowUiCanvas(UIName.CharacterSellection);
        CharacterSellectionScreen.instance.WriteInfo();
    }

    [Binding]
    public void LoadGame()
    {
        UndestructableData.instance.launchMode = LaunchMode.Dungeon;
        LoadingScreen.instance.StartLoading(1);
    }

    [Binding]
    public void Arena()
    {
        DataSaveLoader.DeleteSaves();
        UndestructableData.instance.launchMode = LaunchMode.Arena;
        LoadingScreen.instance.StartLoading(1);
    }

    [Binding]
    public void Exit()
    {
        Application.Quit();
    }
}