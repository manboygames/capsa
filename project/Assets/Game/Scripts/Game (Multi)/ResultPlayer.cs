using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResultPlayer : MonoBehaviour {

    public GameObject winIcon;
    public Text textPlayerName;
    public Text textScore;

    public Color normalNameColor;
    public Color localNameColor;

    public void SetPlayer(string playerName, int score, bool win)
    {
        textPlayerName.text = playerName;
        textScore.text = score.ToString();
        winIcon.SetActive(win);

        /*
        Debug.Log(playerName + " is local: " + isLocal);

        if(isLocal)
        {
            textPlayerName.color = localNameColor;        }
        else
        {
            textPlayerName.color = normalNameColor;
        }*/
    }
}
