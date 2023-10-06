using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI popUpText;
    [SerializeField] private Animator animator;
    [SerializeField] private Image image;



    public void Draw(string text, Color color, Sprite sprite = null, float animatorSpeed = 1)
    {


        popUpText.text = text;

        animator.speed = animatorSpeed;
        image.sprite = sprite;
        if (image.sprite != null) image.color = Color.white;
        else image.color = Color.clear;
        popUpText.color = color;

        animator.Play("damagePopupAnimation");
        StartCoroutine(enumerator());
        IEnumerator enumerator()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0).Length);
            Pooler.Put(gameObject);
        }
    }

}
