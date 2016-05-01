using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Hand))]
public class GamePlayer : NetworkBehaviour {

    #region Enums
    
    public enum TurnState
    {
        Waiting,
        Thinking,
        Playing
    }

    #endregion

    //sync
    [Header("SyncVar")]

    [SyncVar(hook = "HookCharacterID")]
    public int characterID;

    [SyncVar(hook = "HookName")]
    public string playerName;

    [SyncVar(hook = "HookTurnState")]
    public TurnState turnState;

    [SyncVar]
    public int playerID = -1;

    [SyncVar]
    public int totalPlayers = -1;

    [SyncVar]
    public bool isSpawned;

    //GUI
    [Header("GUI")]
    public List<TurnStateInfo> turnStates = new List<TurnStateInfo>();
    private Dictionary<TurnState, Sprite> turnStatesDict = new Dictionary<TurnState, Sprite>();
    public TextMesh playerNameText;
    public GameCharacter character;
    public SpriteRenderer turnStateIndicator;
    public TextMesh turnStateText;

    //private
    protected NetworkClient client;
    protected Hand hand;
    protected GameManager gameManager;
    protected GameUIController uiManager;
    protected EmojiController emojiController;
    protected ChatController chatController;

    public Hand Hand
    {
        get
        {
            return hand;
        }

        set
        {
            hand = value;
        }
    }

    public EmojiBubble emojiBubble;
    public ChatBubble chatBubble;
    public PlayerChair chair;

    void Awake()
    {
        hand = GetComponent<Hand>();
        client = LobbyManager.Instance.client;

        int length = turnStates.Count;
        for (int i = 0; i < length; i++)
        {
            turnStatesDict.Add(turnStates[i].state, turnStates[i].sprite);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        HookCharacterID(characterID);
        HookName(playerName);
        HookTurnState(turnState);
    }

    protected virtual IEnumerator Start()
    {
        while (GameManager.Instance == null)
        {
            yield return null;
        }

        gameManager = GameManager.Instance;

        while(GameUIController.Instance == null)
        {
            yield return null;
        }

        uiManager = GameUIController.Instance;

        while(EmojiController.Instance == null)
        {
            yield return null;
        }

        emojiController = EmojiController.Instance;

        while (ChatController.Instance == null)
        {
            yield return null;
        }

        chatController = ChatController.Instance;

        //if (isClient)
        //{
            gameManager.AddPlayer(this);
        //}
    }

    public void ResetPlayer()
    {
        character.SetLooping(false);
        character.SetEmotion(Character.Emotion.Normal);
        hand.ResetHand();
    }

    #region Hooks

    public void HookName(string newName)
    {
        playerName = newName;
        playerNameText.text = playerName;
    }

    public void HookCharacterID(int newID)
    {
        characterID = newID;
        character.SetCharacter(CharacterManager.Instance.characters[characterID]);
    }

    public void HookTurnState(TurnState newState)
    {
        turnState = newState;
        turnStateIndicator.sprite = turnStatesDict[turnState];
        turnStateText.text = Enum.GetName(typeof(TurnState), turnState);

        if(newState == TurnState.Thinking)
        {
            StartCoroutine(ShowTurnIndicator());
        }
    }

    private IEnumerator ShowTurnIndicator()
    {
        while(GameUIController.Instance == null)
        {
            yield return null;
        }

        GameUIController.Instance.ShowPlayerInfo(chair.indicator.position, chair.GetChairPosition(),  "<color=magenta>" + playerName + "</color>'s turn");
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
    public void CmdChangeTurnState(TurnState newState)
    {
        turnState = newState;
    }

    #endregion

    #region RPC

    [ClientRpc]
    public void RpcResetPlayer()
    {
        hand.ResetHand();
    }

    [ClientRpc]
    public void RpcSetEmotion(Character.Emotion emotion)
    {
        StartCoroutine(SetEmotion(emotion));
    }

    [ClientRpc]
    public void RpcPlayEmoji(string emojiName)
    {
        emojiBubble.SetEmoji(emojiController.GetSprite(emojiName));
        emojiBubble.ShowBubble();
        chatBubble.StopTween();
    }

    public IEnumerator SetEmotion(Character.Emotion emotion, bool looping = false, float delay = 0.2f)
    {
        yield return new WaitForSeconds(delay);
        character.SetLooping(looping);
        character.SetEmotion(emotion);
    }

    [ClientRpc]
    public void RpcShowChat(string chatMsg)
    {
        chatBubble.SetText(chatMsg);
        chatBubble.ShowBubble(2.5f);
        emojiBubble.StopTween();
    }

    #endregion

    public virtual void AssignTurn(bool isTurn, bool firstTurn)
    {
        if (isTurn)
        {
            CmdChangeTurnState(TurnState.Thinking);
        }
        else
        {
            CmdChangeTurnState(TurnState.Waiting);
        }
    }

    protected virtual HandType PlayHand(bool newRound, bool requireLowestCard)
    {
        HandType handType = HandEvaluator.EvaluateHand(hand.PlayedHandData);

        if (handType == HandType.Invalid) //if hand doesnt make sense
        {
            return HandType.Invalid;
        }

        if (requireLowestCard)
        {
            if (!hand.PlayedHandData.ContainsLowestCard(gameManager.lowestCardValue))
            {
                return HandType.Invalid;
            }
        }

        if (!newRound)
        {
            if(gameManager.currentPlayedHand.Count != hand.PlayedHandData.Count)
            {
                return HandType.Invalid;
            }
        }

        List<CardData> gameHand = gameManager.currentPlayedHand.ToDynList();

        if (!HandEvaluator.CompareHand(hand.PlayedHandData, gameHand)) //if hand lose
        {
            return HandType.Invalid;
        }

        hand.RemainingCards -= hand.PlayedHandData.Count;

        return handType;
    }

    protected virtual void PassTurn()
    {
        CmdChangeTurnState(TurnState.Waiting);

        var msg = new ClientPassTurnMsg();
        client.Send(ClientPassTurnMsg.msgID, msg);
    }

    void OnDestroy()
    {
        if (NetworkServer.active)
        {
            GameManager.Instance.OnPlayerDestroyed(this);
        }
        else
        {
            GameManager.Instance.RemovePlayer(this);
        }
    }
}

[System.Serializable]
public struct TurnStateInfo
{
    public GamePlayer.TurnState state;
    public Sprite sprite;
}
