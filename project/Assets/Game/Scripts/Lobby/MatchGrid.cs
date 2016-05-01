using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

public class MatchGrid : MonoBehaviour {

    public Text matchIDText;
    public Text matchNameText;
    public Text matchStatusText;
    public Text matchPopulationText;

    //private NetworkID networkID;
    private string matchName;

    public void SetMatchDescription(MatchDesc matchDesc)
    {
        matchName = matchDesc.name;

        matchPopulationText.text = matchDesc.currentSize.ToString() + "/" + matchDesc.maxSize.ToString();

        if(matchDesc.currentSize == matchDesc.maxSize)
        {
            matchStatusText.text = "Full";
        }

        //matchIDText.text = networkID.ToString();
        matchNameText.text = matchName;
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
    }
}
