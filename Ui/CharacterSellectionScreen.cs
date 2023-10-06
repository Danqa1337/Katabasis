using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityWeld.Binding;

[Binding]
public class CharacterSellectionScreen : Singleton<CharacterSellectionScreen>
{
    public string[] avaibleCahracterNames;
    public Label Description;
    public Label Name;
    public RenderTexture portraitTexture;
    public CircularList<Entity> characters = new CircularList<Entity>();

    private void Awake()
    {
        SimpleObjectsDatabase.instance.StartUp();
        Pooler.UpdatePools();
        LocationMap.Clear();
        SpawnEntities();
    }

    private void SpawnEntities()
    {
        for (int i = 0; i < avaibleCahracterNames.Length; i++)
        {
            characters.Add(new int2(3 * i, 0).ToTileData().Spawn(avaibleCahracterNames[i].DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>()));
        }

        ManualSystemUpdater.Update<AnatomySystem>();
        ManualSystemUpdater.Update<EquipmentSystem>();
        ManualSystemUpdater.Update<InventorySystem>();
    }

    [Binding]
    public void Next()
    {
        characters.Next();
        WriteInfo();
    }

    [Binding]
    public void Prev()
    {
        characters.Prev();
        WriteInfo();
    }

    [Binding]
    public void Back()
    {
        UiManager.ShowUiCanvas(UIName.CharacterSellection);
    }

    public void WriteInfo()
    {
        var itemName = characters.Current.GetComponentData<SimpleObjectNameComponent>().simpleObjectName;
        Description.SetText(LocalizationManager.GetDescription(itemName));
        Name.SetText(LocalizationManager.GetName(itemName));
        PhotoCamera.MakeFullPhoto(characters.Current, portraitTexture);
    }

    [Binding]
    public void StartNewGame()
    {
        UndestructableData.instance.playerRace = characters.Current.GetComponentData<ComplexObjectNameComponent>().complexObjectName;
        UndestructableData.instance.launchMode = LaunchMode.Dungeon;
        DataSaveLoader.DeleteSaves();
        MainMenuButtons.instance.LoadGame();
    }
}