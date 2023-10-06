using UnityEngine;

public class PlayerSquadsScreen : Singleton<PlayerSquadsScreen>
{
    public RenderTexture[] renderTextures;

    private void OnEnable()
    {
        Spawner.OnPlayersSquadSpawned += UpdatePortraits;
        TimeController.OnTurnEnd += UpdatePortraits;
    }
    private void OnDisable()
    {
        Spawner.OnPlayersSquadSpawned -= UpdatePortraits;
        TimeController.OnTurnEnd -= UpdatePortraits;
    }
    public static void UpdatePortraits()
    {
        var playersSquad = Registers.SquadsRegister.GetPlayersSquad();

        var oldPortraits = instance.transform.GetComponentsInChildren<SquadmatePortrait>();
        for (int i = 0; i < oldPortraits.Length; i++)
        {
            Pooler.Put(oldPortraits[i].gameObject);

        }

        for (int i = 1; i < playersSquad.members.Count; i++)
        {
            var squadmate = playersSquad.members[i];
            var portrait = Pooler.Take("SquadmatePortrait", Vector3.zero).GetComponent<SquadmatePortrait>();
            portrait.RenderTexture = instance.renderTextures[i - 1];
            portrait.transform.SetParent(instance.transform);
            portrait.transform.localScale = Vector3.one;
            PhotoCamera.MakeFullPhoto(squadmate, portrait.RenderTexture);



        }
    }

}
