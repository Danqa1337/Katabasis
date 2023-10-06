using TMPro;
using UnityEngine;

public class VersionTexst : MonoBehaviour
{

    private void Start()
    {
        GetComponent<TextMeshProUGUI>().text = "V " + Application.version;
    }
}
