using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class LoadingScreen : Singleton<LoadingScreen>
{
    private Canvas _canvas;
    [SerializeField] private Label AdviceLabel;
    private bool isShown;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        Hide();
    }

    public static async Task Show()
    {
        //if (!instance.isShown)
        //{
        //    instance._canvas.enabled = true;
        //    instance._canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //    instance.isShown = true;
        //    var hintsTable = LocalizationSettings.StringDatabase.GetTableAsync("Hints").WaitForCompletion();
        //    instance.AdviceLabel.SetText(hintsTable.Values.ToArray()[(int)UnityEngine.Random.Range(0, hintsTable.Count - 1)].GetLocalizedString());
        //    await Task.Delay(100);
        //}
    }

    public async void StartLoading(int sceneNum)
    {
        Show();
        var operation = SceneManager.LoadSceneAsync(sceneNum);
        while (!operation.isDone)
        {
            await Task.Delay(10);
        }
    }

    public void EndLoading()
    {
        Hide();
    }

    public static void Hide()
    {
        //isShown = false;
        //_canvas.renderMode = RenderMode.WorldSpace;
        //_canvas.enabled = false;
    }
}