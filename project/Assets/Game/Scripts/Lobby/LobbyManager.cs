using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class LobbyManager : NetworkLobbyManager
{
    private static LobbyManager instance;

    public static LobbyManager Instance
    {
        get
        {
            return instance;
        }
    }

    private bool isMatchMaking;

    public bool IsMatchMaking
    {
        get
        {
            return isMatchMaking;
        }

        set
        {
            isMatchMaking = value;
        }
    }
    public InfoPanel infoPanel;
    public Button backButton;
    public GameObject panelLobby;
    public LobbyMenu lobbyMenu;
    public MainMenu mainMenu;
    public GameListMenu gameListMenu;
    public List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();

    private LobbyHook lobbyHook;

    private ulong currentMatchID;
    [HideInInspector]
    public ulong currentNodeID;
    private bool disconnectServer;

    #region Events and Callbacks

    public delegate void BackDelegate();
    public BackDelegate backDelegate;

    [System.Serializable]
    public class OnClientConnectEvent : UnityEvent<NetworkConnection> { }

    [HideInInspector]
    public OnClientConnectEvent onClientConnect = new OnClientConnectEvent();

    [System.Serializable]
    public class OnClientDisconnectEvent : UnityEvent<NetworkConnection> { }

    [HideInInspector]
    public OnClientDisconnectEvent onClientDisconnect = new OnClientDisconnectEvent();

    [System.Serializable]
    public class OnLobbyPlayerCreated : UnityEvent<GameObject> { }

    [HideInInspector]
    public OnLobbyPlayerCreated onLobbyPlayerCreated = new OnLobbyPlayerCreated();

    #endregion

    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        lobbyHook = GetComponent<LobbyHook>();
        backDelegate = Back_MainScene;
    }

    void OnLevelWasLoaded(int level)
    {
        if (LobbyManager.Instance != this)
        {
            return;
        }

        Debug.Log("OnLevelWasLoaded");

        if (SceneManager.GetActiveScene().name == lobbyScene)
        {
            panelLobby.SetActive(true);

            if (NetworkServer.active)
            {
                backDelegate = Back_StopHost;
            }
            else
            {
                backDelegate = Back_StopClient;
            }
        }
    }

    #region Lobby

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("OnLobbyServerCreateLobbyPlayer");

        var go = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;
        var lp = go.GetComponent<LobbyPlayer>();
        AddLobbyPlayer(lp);

        return go;
    }

    public override void OnLobbyServerPlayersReady()
    {
        //if (numPlayers == maxPlayers)
        /*if (numPlayers > 1)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if(!lobbyPlayers[i].readyToBegin)
                {
                    lobbyPlayers[0].AllowServerStart(false);
                    return;
                }
            }

            lobbyPlayers[0].AllowServerStart(true);
        }
        else
        {
            lobbyPlayers[0].AllowServerStart(false);
        }*/

        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (!lobbyPlayers[i].readyToBegin)
            {
                lobbyPlayers[0].AllowServerStart(false);
                return;
            }
        }

        lobbyPlayers[0].AllowServerStart(true);
    }

    public override void OnMatchCreate(CreateMatchResponse matchInfo)
    {
        base.OnMatchCreate(matchInfo);
        Debug.Log("OnMatchCreate");
        currentMatchID = (System.UInt64)matchInfo.networkId;
        currentNodeID = (System.UInt64)matchInfo.nodeId;
    }

    //this is hack because unity OnMatchJoined not virtual...
    public virtual void MatchJoined(JoinMatchResponse matchInfo)
    {
        if (matchInfo.success)
        {
            try
            {
                Utility.SetAccessTokenForNetwork(matchInfo.networkId, new NetworkAccessToken(matchInfo.accessTokenString));
            }
            catch (System.ArgumentException ex)
            {
                if (LogFilter.logError)
                {
                    Debug.LogError (ex);
                }
            }
            StartClient(new MatchInfo(matchInfo));
        }
        else if (LogFilter.logError)
        {
            Debug.LogError(string.Concat("Join Failed:", matchInfo));
        }
    }


    public override void OnLobbyServerDisconnect(NetworkConnection conn)
    {
        //This is called on the server when a client disconnects.
        base.OnLobbyServerDisconnect(conn);
        Debug.Log("OnLobbyServerDisconnect");
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("OnStartHost");
        backDelegate = Back_StopHost;
    }

    public override void OnLobbyStartHost()
    {
        base.OnLobbyStartHost();
        Debug.Log("OnLobbyStartHost");
    }

    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();
        Debug.Log("OnLobbyStopHost");
    }

    public override void OnLobbyStopClient()
    {
        base.OnLobbyStopClient();
        Debug.Log("OnLobbyStopClient");

        RefreshAfterDisconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //Called on the server when a client disconnects.
        base.OnServerDisconnect(conn);
        Debug.Log("OnServerDisconnect");

        onClientDisconnect.Invoke(conn);

        /*if (numPlayers < 2)
        {
            Back_StopHost();
        }
        else
        {
            onClientDisconnect.Invoke(conn);
        }*/
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        Debug.Log("OnLobbyServerSceneLoadedForPlayer");

        if (lobbyHook)
        {
            lobbyHook.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);
        }

        return true;
    }

    public override void OnStartClient(NetworkClient lobbyClient)
    {
        base.OnStartClient(lobbyClient);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("OnClientConnect: " + conn);

        onClientConnect.Invoke(conn);

        if (!NetworkServer.active)
        {
            backDelegate = Back_StopClient;
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("OnClientDisconnect: " + conn);
        backButton.interactable = true;

        //gameListMenu.ClearGameList();
        //mainMenu.UIRefreshList();
    }

    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        //including other lobby clients
        base.OnLobbyClientConnect(conn);
        Debug.Log("OnLobbyClientConnect: " + conn);
    }

    public override void OnLobbyClientDisconnect(NetworkConnection conn)
    {
        //This is called on the client when disconnected from a server.
        base.OnLobbyClientDisconnect(conn);
        Debug.Log("OnLobbyClientDisconnect: " + conn);

        matchMaker.DropConnection((NetworkID)currentMatchID, (NodeID)currentNodeID, OnMatchmakerDrop);
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        Debug.Log("OnLobbyClientSceneChanged");
        if (SceneManager.GetSceneAt(0).name == playScene)
        {
            panelLobby.SetActive(false);
        }
        else if(SceneManager.GetSceneAt(0).name == lobbyScene)
        {
            panelLobby.SetActive(true);
        }
    }

    #endregion

    #region Public

    public void AddLobbyPlayer(LobbyPlayer lobbyPlayer)
    {
        lobbyPlayers.Add(lobbyPlayer);
    }

    public void RemoveLobbyPlayer(LobbyPlayer lobbyPlayer)
    {
        if (lobbyPlayers.Contains(lobbyPlayer))
        {
            lobbyPlayers.Remove(lobbyPlayer);
        }
    }

    public IEnumerator StartGame(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        //TODO: MAKE COUNTDOWN
        ServerChangeScene(playScene);
    }

    #endregion

    #region UI

    private void Back_MainScene()
    {
        //Destroy(GameObject.Find("CanvasLobby"));
        SceneManager.LoadScene("Selection");
        Destroy(gameObject);
    }

    private void Back_StopClient()
    {
        backButton.interactable = false;
        matchMaker.DropConnection((NetworkID)currentMatchID, (NodeID)currentNodeID, OnMatchmakerDrop);
    }

    private void OnMatchmakerDrop(BasicResponse response)
    {
        Debug.Log("OnMatchmakerDrop: " + response.success + ", " + response.extendedInfo);
        StopClient();

        RefreshAfterDisconnect();
    }

    private void Back_StopHost()
    {
        backButton.interactable = false;
        disconnectServer = true;
        matchMaker.DestroyMatch((NetworkID)currentMatchID, OnMatchDestroyed);
    }

    private void OnMatchDestroyed(BasicResponse response)
    {
        Debug.Log("OnMatchDestroyed: "+response.success+", "+response.extendedInfo);
        if(disconnectServer)
        {
            //matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, OnMatchmakerDrop);
            StopMatchMaker();
            StopHost();
            disconnectServer = false;
        }

        RefreshAfterDisconnect();
    }

    private void RefreshAfterDisconnect()
    {
        mainMenu.SetCurrentCanvasGroup(mainMenu.mainGroup, true);
        backDelegate = Back_MainScene;
        backButton.interactable = true;

        mainMenu.RefreshList(3);
    }

    #endregion

}
