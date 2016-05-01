using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyPlayer : NetworkLobbyPlayer
{
    [SyncVar(hook = "HookName")]
    public string playerName;

    [SyncVar(hook = "HookID")]
    public int characterID;

    [SyncVar(hook = "HookReady")]
    public bool isReady;

    [SyncVar(hook = "HookStatus")]
    public bool isHost;

    public Text playerNameText;
    public Button startButton;
    public Toggle readyToggle;
    public Image hostImage;
    public SpriteRenderer characterSprite;
    private Text statusText;

    private Character character;
    private LobbyManager lobbyManager;

    private bool initialized;
    private CanvasGroup canvasGroup;

    #region Initialization

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = canvasGroup.blocksRaycasts = characterSprite.enabled = false;
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        //LobbyMenu.Instance.SetPlayerLobbyPosition(gameObject);

        lobbyManager = LobbyManager.Instance;
        lobbyManager.lobbyMenu.SetPlayerLobbyPosition(gameObject);

        character = GetComponentInChildren<Character>();
        statusText = readyToggle.GetComponentInChildren<Text>();
        Debug.Log("OnClientEnterLobby");

        HookName(playerName);
        HookID(characterID);
        HookStatus(isHost);
        HookReady(isReady);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("OnStartLocalPlayer");

        if (CharacterManager.Instance)
        {
            CmdChangeName(CharacterManager.Instance.characterName);
            CmdChangeCharacterID(CharacterManager.Instance.characterID);
        }

        CmdChangeReadyStatus(false);
        CmdChangeHostStatus(isServer);

        if (isLocalPlayer)
        {
            HookReady(false);

            if (NetworkServer.active)
            {
                readyToggle.gameObject.SetActive(false);
                startButton.gameObject.SetActive(true);
                startButton.interactable = false;
                hostImage.gameObject.SetActive(false);
                if (!readyToBegin)
                {
                    SendReadyToBeginMessage();
                }
            }
            else
            {
                readyToggle.gameObject.SetActive(true);
                startButton.gameObject.SetActive(false);
                readyToggle.interactable = true;
                SendNotReadyToBeginMessage();
            }
        }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log("OnStartAuthority");
    }

    #endregion

    #region Hooks

    public void HookName(string newName)
    {
        playerName = newName;
        playerNameText.text = playerName;
    }

    public void HookID(int newID)
    {
        characterID = newID;
        character.SetCharacter(CharacterManager.Instance.characters[characterID]);

        if (!initialized)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            characterSprite.enabled = true;
            readyToggle.onValueChanged.AddListener(OnReadyChanged);
            initialized = true;
        }
    }

    public void HookReady(bool readyState)
    {
        isReady = readyState;
        readyToggle.isOn = readyState;

        statusText.text = isReady ? "Ready" : "Waiting";
    }

    public void HookStatus(bool hostStatus)
    {
        isHost = hostStatus;

        if(!NetworkServer.active)
        {
            hostImage.gameObject.SetActive(hostStatus);
        }
        else
        {
            hostImage.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Commands

    [Command]
    public void CmdChangeName(string newName)
    {
        playerName = newName;
    }

    [Command]
    public void CmdChangeCharacterID(int newID)
    {
        characterID = newID;
    }

    [Command]
    public void CmdChangeReadyStatus(bool newStatus)
    {
        isReady = newStatus;
    }

    [Command]
    public void CmdChangeHostStatus(bool hostStatus)
    {
        isHost = hostStatus;
    }

    #endregion

    #region Events

    private void OnReadyChanged(bool isOn)
    {
        Debug.Log("OnReadyChanged: "+isOn);
        CmdChangeReadyStatus(isOn);

        if (isOn)
        {
            SendReadyToBeginMessage();
        }
        else
        {
            SendNotReadyToBeginMessage();
        }
    }

    /*public override void OnClientReady(bool readyState)
    {

    }*/

    #endregion

    #region Public

    public void AllowServerStart(bool allow)
    {
        startButton.interactable = allow;
    }

    public void StartGame()
    {
        if (isServer)
        {
            StartCoroutine(lobbyManager.StartGame());
        }
    }

    #endregion

    #region Private

    void OnDestroy()
    {
        Debug.Log("Lobby player destroyed");
        lobbyManager.RemoveLobbyPlayer(this);
    }

    #endregion
}
