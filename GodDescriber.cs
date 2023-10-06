using Gods;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GodDescriber : MonoBehaviour
{
    [SerializeField] private Label _descriptionLabel;
    [SerializeField] private Label _nameLabel;
    [SerializeField] private Label _relationsLabel;
    [SerializeField] private Label _attentionLabel;
    [SerializeField] private GodIcon _icon;
    [SerializeField] private KatabasisButton _prayButton;
    [SerializeField] private TextMeshProUGUI _indexText;
    private int _index = -1;

    private void OnEnable()
    {
        GodIcon.OnLeftClickStatic += Describe;
        UiManager.OnShowCanvas += ListenUi;
    }

    private void OnDisable()
    {
        GodIcon.OnLeftClickStatic -= Describe;
        UiManager.OnShowCanvas -= ListenUi;
    }

    private void ListenUi(UIName uIName)
    {
        if (uIName == UIName.Gods)
        {
            Redraw();
        }
    }

    private void Redraw()
    {
        Describe(_index);
    }

    public void Describe(int index)
    {
        Debug.Log("Describing god info " + index);
        _index = index;
        if (_index != -1)
        {
            _index = index;
            var god = Registers.GodsRegister.GetGod(index);
            var GodArchetype = god.GodArchetype;
            if (god.AttentionLevel != GodAttentionLevel.Min)
            {
                _icon.Draw(god);
                _indexText.text = god.Index.ToString();
                _descriptionLabel.SetText(LocalizationManager.GetString("GodsDescriptions", GodArchetype.ToString()));
                _nameLabel.SetText(god.Name);
                _relationsLabel.SetText(god.Relations.ToString());
                _attentionLabel.SetText(LocalizationManager.GetString("AttentionLevels", god.AttentionLevel.ToString()));
                _prayButton.interactable = true;
            }
            else
            {
                Clear();
            }
        }
        else
        {
            Clear();
        }
    }

    private void Clear()
    {
        _icon.Clear();
        _nameLabel.SetText("...");
        _relationsLabel.SetText("...");
        _attentionLabel.SetText("...");
        _prayButton.interactable = false;

        if (Registers.GodsRegister.GetAllGods().All(god => (int)god.AttentionLevel < 1))
        {
            _descriptionLabel.SetText(LocalizationManager.GetString("No Gods Visible"));
        }
        else
        {
            _descriptionLabel.SetText("...");
        }
    }

    public void Pray()
    {
        if (_index != -1)
        {
            StartCoroutine(ProcessPrayer());

            IEnumerator ProcessPrayer()
            {
                UiManager.CloseLast();
                MainCameraHandler.CenterCameraOnTile(Player.CurrentTile);
                Controller.ChangeActionMap(ActionMap.Menuing);
                yield return new WaitForSeconds(2);

                Registers.GodsRegister.GetGod(_index).OnPray();
                TimeController.SpendTime(10);
                Controller.ChangeActionMap(ActionMap.Default);
            }
        }
    }
}