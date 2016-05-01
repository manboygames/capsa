using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AdvancedInspector;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class MainMenu : MonoBehaviour
{
    private static MainMenu instance;

    public static MainMenu Instance
    {
        get
        {
            return instance;
        }

        set
        {
            instance = value;
        }
    }

    private LobbyManager lobbyManager;

    [Inspect, Group("Main")]
    public CanvasGroup mainGroup;
    [Inspect, Group("Main")]
    public InputField matchNameField;
    [Inspect, Group("Main")]
    public GameListMenu gameListMenu;

    //[Inspect, Group("Info")]
    public InfoPanel infoPanel;
    public CanvasGroup roomGroup;

    private CanvasGroup currentGroup;

    private float refreshMatchFreq = 10f;

    void Awake()
    {
        instance = this;
        currentGroup = mainGroup;
    }

    IEnumerator Start()
    {
        while (LobbyManager.Instance == null)
        {
            yield return null;
        }

        lobbyManager = LobbyManager.Instance;

        //Invoke("ListMatches", 1);
        //CancelInvoke("ListMatches");
        InvokeRepeating("ListMatches", 3, refreshMatchFreq);
    }

    private void SetCanvasGroupActive(CanvasGroup cg, bool isActive)
    {
        cg.alpha = isActive ? 1 : 0;
        cg.interactable = isActive;
        cg.blocksRaycasts = isActive;
    }

    public void SetCurrentCanvasGroup(CanvasGroup newCg, bool deactivateCurrent)
    {
        if (deactivateCurrent)
        {
            SetCanvasGroupActive(currentGroup, false);
        }

        currentGroup = newCg;
        SetCanvasGroupActive(currentGroup, true);
    }

    #region UI

    public void BackButton()
    {
        LobbyManager.Instance.backDelegate();
    }

    public void CreateGame()
    {
        if (matchNameField.text.Length > 0)
        {
            gameListMenu.ClearGameList();
            CancelInvoke("ListMatches");

            SetCanvasGroupActive(mainGroup, false);
            lobbyManager.backButton.interactable = false;

            lobbyManager.StartMatchMaker();
            /*CreateMatchRequest options = new CreateMatchRequest();
            options.name = matchNameField.text;
            options.size = (uint)lobbyManager.maxPlayers;
            options.advertise = true;
            options.password = string.Empty;*/
            lobbyManager.matchMaker.CreateMatch(matchNameField.text, (uint)lobbyManager.maxPlayers, true, string.Empty, lobbyManager.OnMatchCreate);

            SetCurrentCanvasGroup(infoPanel.canvasGroup, true);

            lobbyManager.onClientConnect.AddListener(OnClientConnect);
        }
    }

    public void JoinMatch(NetworkID networkID)
    {
        gameListMenu.ClearGameList();
        CancelInvoke("ListMatches");
        lobbyManager.onClientConnect.AddListener(OnClientConnect);
        lobbyManager.backButton.interactable = false;
        //lobbyManager.matchMaker.JoinMatch(networkID, "", lobbyManager.OnMatchJoined);
        lobbyManager.matchMaker.JoinMatch(networkID, "", lobbyManager.MatchJoined);
        SetCurrentCanvasGroup(infoPanel.canvasGroup, true);
    }

    public void UIRefreshList()
    {
        gameListMenu.ClearGameList();
        RefreshList(3);
    }

    public void RefreshList(float delay)
    {
        CancelInvoke("ListMatches");
        InvokeRepeating("ListMatches", delay, refreshMatchFreq);
    }

    private void ListMatches()
    {
        lobbyManager.StartMatchMaker();
        lobbyManager.matchMaker.ListMatches(0, 6, "", OnMatchList);
        //lobbyManager.IsMatchMaking = true;
    }

    #endregion

    #region Listeners

    private void OnClientConnect(NetworkConnection conn)
    {
        CancelInvoke("ListMatches");
        lobbyManager.backButton.interactable = true;

        if (lobbyManager.client.connection == conn)
        {
            lobbyManager.onClientConnect.RemoveListener(OnClientConnect);
            SetCurrentCanvasGroup(roomGroup, true);
        }
    }

    private void OnMatchList(ListMatchResponse response)
    {
        if(response.matches.Count == 0)
        {
            gameListMenu.ClearGameList();
        }

        for (int i = 0; i < response.matches.Count; i++)
        {
            /*if(response.matches[0].hostNodeId == (NodeID)lobbyManager.currentNodeID)
            {
                Debug.Log("Host is me");
                continue;
            }*/
            gameListMenu.Populate(response.matches[i]);
        }
    }

    #endregion

    void OnDestroy()
    {
        if(lobbyManager)
        {
            lobbyManager.onClientConnect.RemoveListener(OnClientConnect);
        }
    }

}
