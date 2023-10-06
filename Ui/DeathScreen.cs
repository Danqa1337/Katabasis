using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

[Binding]
public class DeathScreen : MonoBehaviour
{
    [SerializeField] private Label PostmortemLabel;

    private void OnEnable()
    {
        Player.OnPlayerDiedWithReason += ShowPostMortem;
    }

    private void OnDisable()
    {
        Player.OnPlayerDiedWithReason -= ShowPostMortem;
    }

    private void ShowPostMortem(Entity responsibleEntity)
    {
        UiManager.ShowUiCanvas(UIName.Death);

        string postMortem = "";
        if (responsibleEntity == Entity.Null || !responsibleEntity.Exists())
        {
            postMortem = "Ты здох как собака.";
        }
        else
        {
            var responsibleEntityName = LocalizationManager.GetName(responsibleEntity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName);
            if (responsibleEntity == Player.PlayerEntity)
            {
                postMortem = "Ты здох как собака.\n Судя по всему ты убил себя сам. ";
            }
            else if (responsibleEntity.HasComponent<CreatureComponent>())
            {
                postMortem = responsibleEntityName + " забил тебя до смерти. \n Ты мертв.";
            }
            else
            {
                postMortem = "Ты мертв.\n Вероятной причной смерти является " + responsibleEntityName;
            }
        }
        PostmortemLabel.SetText(postMortem);
    }

    [Binding]
    public async void Quit()
    {
        WorldDisposer.DisposeWorld();
        await LoadingScreen.Show();
        SceneManager.LoadScene(0);
    }
}