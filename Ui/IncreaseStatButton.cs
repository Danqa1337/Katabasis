using UnityEngine;
using UnityEngine.UI;

public class IncreaseStatButton : MonoBehaviour
{
    private Button _button;

    protected void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Increace);
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            _button.interactable = Registers.StatsRegister.FreeStatPoints > 0;
            if (_button.interactable)
            {
                var sin = Mathf.Abs(Mathf.Sin(Time.time * 2));
                _button.targetGraphic.color = Color.Lerp(Color.gray, Color.white, sin);
            }
        }
    }

    private void Increace()
    {
    }
}