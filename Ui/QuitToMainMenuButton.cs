using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitToMainMenuButton : MonoBehaviour
{


    public void Play()
    {

        WorldDisposer.DisposeWorld();
        SceneManager.LoadScene(0);

    }

}
