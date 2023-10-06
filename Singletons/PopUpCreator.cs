using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public enum PopupType
{
    Fear,
    Interaction,
    Bleeding,
    NoAmmo,
    Death,
    Parry,
    Evade,
    jump,
    Healing,
    Yes,
    No,
    Locked,
    Unlocked,
}
public class PopUpCreator : Singleton<PopUpCreator>
{
    //static Queue<PopUp> sheludedPopups;
    public int delayBetweenPopUps;
    public Color damagePopupColor;
    public Color healPopupColor;
    public Color imagePopupColor;
    private bool _ready = true;

    async static private void CreatePopUp(float2 position, Color textColor, string text = "", Sprite sprite = null, float offset = 1.5f, float animationSpeed = 1)
    {



        while (instance._ready == false)
        {
            await Task.Delay(instance.delayBetweenPopUps);
        }

        instance._ready = false;

        if (position.ToTileData().visible)
        {
            var popupObject = Pooler.Take("PopUp", (position + UnityEngine.Random.insideUnitCircle.ToFloat2() * 0.25f).ToRealPosition());
            if (popupObject != null)
            {
                PopUp newPopUp = popupObject.GetComponent<PopUp>();


                newPopUp.Draw(text, textColor, sprite, animationSpeed);
            }


        }
        instance._ready = true;

    }

    async static private void CreatePopUp(float2 position, Sprite sprite, float offset = 1.5f, float animationSpeed = 1)
    {
        CreatePopUp(position, Color.clear, "", sprite, offset, animationSpeed);
    }
    async static public void CreatePopUp(string text)
    {
        CreatePopUp(Player.PlayerEntity.CurrentTile().position, Color.gray, text, null);
    }
    async static public void CreatePopUp(float2 position, string text)
    {
        CreatePopUp(position, Color.gray, text, null);
    }
    async static public void CreatePopUp(float2 position, string text, Color color, float offset = 1.5f, float animationSpeed = 1)
    {
        CreatePopUp(position, color, text, null, offset, animationSpeed);
    }

    async static public void CreatePopUp(float2 position, PopupType popupType, float offset = 1.5f)
    {
        CreatePopUp(position, IconDataBase.GetPopupIcon(popupType), offset);
    }



}
