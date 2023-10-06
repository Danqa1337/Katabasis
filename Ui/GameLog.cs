using TMPro;
using UnityEngine;

public class GameLog : MonoBehaviour
{
    private TextMeshProUGUI TextMesh;
    private int turn;

    private static GameLog _instance;
    public static GameLog i
    {
        get
        {

            return _instance;
        }
    }
    private void Start()
    {
        _instance = this;
        TextMesh = GetComponent<TextMeshProUGUI>();
    }
    int lines;
    public int maxLines;
    public void NewLine(string String)
    {

        if (lines > maxLines)
        {
            string oldText = TextMesh.text;
            int index = oldText.IndexOf("\n");
            //Debug.Log(index);
            TextMesh.text = oldText.Substring(index + System.Environment.NewLine.Length);
            lines--;
        }

        TextMesh.text = TextMesh.text + String + "\n ";
        lines++;
        //Debug.Log("Inserted");
    }

    //private void FixedUpdate()
    //{
    //    if ((int)TickSystem.i.currentTurn > turn + 1)
    //    {

    //        turn = (int)TickSystem.i.currentTurn;

    //            TextMesh.text.


    //    }

    //}
}
