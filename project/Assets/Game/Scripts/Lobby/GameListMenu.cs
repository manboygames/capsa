using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class GameListMenu : MonoBehaviour {

    public MainMenu mainMenu;
    public SpawningPool pool;
    public Transform container;

    public void Populate(MatchDesc matchDesc)
    {
        ClearGameList();

        NetworkID networkID = matchDesc.networkId;
        GameObject grid = pool.GetUnit();
        grid.SetActive(true);
        grid.transform.SetParent(container);
        grid.transform.localScale = new Vector3(1, 1, 1);
        grid.GetComponent<MatchGrid>().SetMatchDescription(matchDesc);

        if(matchDesc.currentSize < matchDesc.maxSize)
        {
            var button = grid.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { JoinMatch(networkID); });
        }
    }

    public void ClearGameList()
    {
        foreach (Transform trans in container)
        {
            pool.ReturnUnit(trans.gameObject);
        }
    }

    private void JoinMatch(NetworkID networkID)
    {
        mainMenu.JoinMatch(networkID);
    }

}
