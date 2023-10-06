using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using static LowLevelSettings;

[Binding]
public class PauseScreen : MonoBehaviour
{
    public Canvas controllsCanvas;
    public Canvas optionsCanvas;

    [Binding]
    public void OpenOptions()
    {
        optionsCanvas.enabled = true;
    }

    [Binding]
    public void CloseOptions()
    {
        optionsCanvas.enabled = false;
    }

    [Binding]
    public void OpenControlls()
    {
        controllsCanvas.enabled = true;
    }

    [Binding]
    public void CloseControlls()
    {
        controllsCanvas.enabled = false;
    }

    [Binding]
    public void SaveAndQuit()
    {
        DataSaveLoader.SaveAll();
        LocationMap.Clear();
        StartCoroutine(IE());
        IEnumerator IE()
        {
            while (DataSaveLoader.IsOperating)
            {
                yield return new WaitForEndOfFrame();
            }
            WorldDisposer.DisposeWorld();
            LoadingScreen.instance.StartLoading(0);
        }
    }

    [Binding]
    public void GiveUp()
    {
        DataSaveLoader.DeleteSaves();
        WorldDisposer.DisposeWorld();
        SceneManager.LoadSceneAsync(0);
    }
}