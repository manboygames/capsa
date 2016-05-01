using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System;
using CodeBureau;

#region Enums
public enum GameState
{
    [StringValue("ServerState_Initialize")]
    Initialize,
    [StringValue("ServerState_AddBots")]
    AddingBots,
    [StringValue("ServerState_SetupPlayers")]
    SetupPlayers,
    [StringValue("ServerState_Reset")]
    Reset,
    [StringValue("ServerState_AssignCards")]
    Assigning,
    [StringValue("ServerState_DealCards")]
    Dealing,
    [StringValue("ServerState_FirstRound")]
    FirstRound,
    [StringValue("ServerState_GameRound")]
    GameRound,
    [StringValue("ServerState_NewRound")]
    NewRound,
    [StringValue("ServerState_GameOver")]
    GameOver
}

#endregion

public class GameManager : NetworkBehaviour
{

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    //Inspector
    public GameObject botPrefab;
    public List<PlayerChair> playerChairsList;
    public Deck deck;
    public GameUIController uiManager;
    public EmojiController emojiController;

    private List<GamePlayer> gamePlayersList = new List<GamePlayer>();
    private List<int> connectionsID = new List<int>();

    private GamePlayer localPlayer = null;
    private GamePlayer currentTurnPlayer = null;
    private GamePlayer currentWinningPlayer = null;
    private GamePlayer previousPlayer = null;
    private int currentPlayerIndex = -1;
    private int totalPlayers = -1;
    private int readyHands = 0;
    private int passTurnAmount = 0;
    private int rematchVotes = 0;
    private bool isPlaying;
    private bool firstTurn = true;

    private int botCounts = 0;

    //Network

    [SyncVar]
    public GameState gameState;

    [SyncVar]
    public HandType handType;

    [SyncVar]
    public int lowestCardValue;

    public SyncListCardData currentPlayedHand = new SyncListCardData();
    public int stackSortingOrder = 0;


    void Awake()
    {
        gameState = GameState.Initialize;
        instance = this;

        foreach (PlayerChair chair in playerChairsList)
        {
            chair.list = playerChairsList;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler(ClientPlayerReadyMsg.msgID, OnClientPlayerReady);
        NetworkServer.RegisterHandler(ClientPlayHandMsg.msgID, OnClientPlayHand);
        NetworkServer.RegisterHandler(ClientPassTurnMsg.msgID, OnClientPassTurn);
        NetworkServer.RegisterHandler(ClientPlayEmojiMsg.msgID, OnClientPlayEmoji);
        NetworkServer.RegisterHandler(ClientSubmitChatMsg.msgID, OnClientChat);
        NetworkServer.RegisterHandler(ClientDestroyedMessage.msgID, OnClientDestroyed);
        NetworkServer.RegisterHandler(ClientRematchMessage.msgID, OnClientRematch);

        LobbyManager.Instance.onClientDisconnect.AddListener(OnClientDisconnect);
    }

    private void OnClientDisconnect(NetworkConnection conn)
    {
        /*Debug.Log("Player disconnected");
        if(LobbyManager.Instance.numPlayers < 2)
        {
            LobbyManager.Instance.backDelegate();
        }

        if (isPlaying)
        {
            var player = conn.playerControllers[0].gameObject.GetComponent<NetworkGamePlayer>();

            if (player.playerID == currentTurnPlayer.playerID)
            {
                Debug.Log("Current player disconnected, next player plays");
                ServerNextPlayer();
                RemovePlayer(player);
            }
        }*/
    }

    #region Events
    [Server]
    private void OnClientPlayerReady(NetworkMessage netMsg)
    {
        Debug.Log("OnClientPlayerReady");
        switch (gameState)
        {
            case GameState.Assigning:
                ServerAddReadyHands();
                break;

            default:
                ServerAddConnection(netMsg.conn.connectionId);
                break;
        }
    }

    [Server]
    private void OnClientPlayHand(NetworkMessage netMsg)
    {
        Debug.Log("OnClientPlayHand");

        var msg = netMsg.ReadMessage<ClientPlayHandMsg>();
        PlayHand(msg.cardDatas, msg.handType, msg.remainingCards, msg.playingSet);
    }

    public void PlayHand(CardData[] cardDatas, HandType handType, int remainingCards, bool playingSet)
    {

        passTurnAmount = 0;
        currentWinningPlayer = currentTurnPlayer;

        if (playingSet) //if this player played a set
        {
            if (previousPlayer != null && previousPlayer != currentTurnPlayer) //if previous 5 cards played by previous player is beaten by this player
            {
                previousPlayer.RpcSetEmotion(Character.Emotion.Sad);
            }
            else
            {
                currentTurnPlayer.RpcSetEmotion(Character.Emotion.Happy);
            }
        }

        ServerUpdatePlayedHand(cardDatas, handType);

        //if not game over
        if (remainingCards > 0)
        {
            previousPlayer = currentTurnPlayer;
            ServerNextPlayer();
        }
        else
        {
            //end game
            ServerNextState(StringEnum.GetStringValue(GameState.GameOver));
        }
    }

    [Server]
    private void OnClientPassTurn(NetworkMessage netMsg)
    {
        PlayerPassTurn();
    }

    public void PlayerPassTurn()
    {
        passTurnAmount++;

        if (passTurnAmount < gamePlayersList.Count - 1)
        {
            ServerNextPlayer();
        }
        else
        {
            passTurnAmount = 0;
            ServerNextState(StringEnum.GetStringValue(GameState.NewRound));
        }
    }

    [Server]
    private void OnClientPlayEmoji(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<ClientPlayEmojiMsg>();
        var player = netMsg.conn.playerControllers[0].gameObject.GetComponent<GamePlayer>();
        player.RpcPlayEmoji(msg.spriteName);
    }

    [Server]
    private void OnClientChat(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<ClientSubmitChatMsg>();
        var player = netMsg.conn.playerControllers[0].gameObject.GetComponent<GamePlayer>();
        player.RpcShowChat(msg.chatMsg);
    }

    [Server]
    public void OnPlayerDestroyed(GamePlayer player)
    {
        if(player == localPlayer)
        {
            return;
        }

        if (LobbyManager.Instance.numPlayers < 2)
        {
            LobbyManager.Instance.backDelegate();
        }

        if (isPlaying)
        {
            if(player == currentTurnPlayer)
            {
                Debug.Log("Current player disconnected, next player plays");
                ServerNextPlayer();
            }
        }

        RemovePlayer(player);
    }

    private void OnClientDestroyed(NetworkMessage netMsg)
    {
        Debug.Log("OnClientDestroyed");

        /*if(isPlaying)
        {
            var msg = netMsg.ReadMessage<ClientDestroyedMessage>();

            if (msg.playerID == currentTurnPlayer.playerID)
            {
                ServerNextPlayer();
            }
        }*/
    }

    [Server]
    private void OnClientRematch(NetworkMessage netMsg)
    {
        Debug.Log("OnClientExit");
        var msg = netMsg.ReadMessage<ClientRematchMessage>();

        if(msg.rematch)
        {
            //TODO: temp rematch
            rematchVotes++;
            if(rematchVotes >= LobbyManager.Instance.numPlayers)
            {
                rematchVotes = 0;
                ServerNextState(StringEnum.GetStringValue(GameState.Reset));
            }
        }
        else
        {
            //if not everyone agrees to rematch
            LobbyManager.Instance.SendReturnToLobby();
        }
    }

    #endregion

    [Server]
    public void ServerAddConnection(int cId)
    {
        connectionsID.Add(cId);
        Debug.Log("connections id length: " + connectionsID.Count);

        if (connectionsID.Count == LobbyManager.Instance.numPlayers)
        {
            if(LobbyManager.Instance.numPlayers < LobbyManager.Instance.maxPlayers)
            {
                botCounts = LobbyManager.Instance.maxPlayers - LobbyManager.Instance.numPlayers;
                ServerNextState(StringEnum.GetStringValue(GameState.AddingBots));
            }
            else
            {
                ServerNextState(StringEnum.GetStringValue(GameState.Assigning));
            }
        }
    }

    [Server]
    public void ServerAddReadyHands()
    {
        readyHands++;

        //if (readyHands == gamePlayersList.Count)
        if(readyHands == LobbyManager.Instance.maxPlayers)
        {
            readyHands = 0;
            ServerState_DealCards();
        }
    }

    [Server]
    private void ServerUpdatePlayedHand(CardData[] datas, HandType handType)
    {
        if(currentPlayedHand.Count > 0)
        {
            CardData[] handArray = currentPlayedHand.ToArray();
            RpcGreyoutStack();
            currentPlayedHand.Clear();
        }

        foreach (CardData cd in datas)
        {
            currentPlayedHand.Add(cd);
        }

        //currentTurnPlayer.Hand.RemoveCardDatas(cardDatas);
        currentTurnPlayer.Hand.RpcMoveCardsToPlay(datas, handType);
    }

    public void SetLocalPlayer(GamePlayer player)
    {
        localPlayer = player;
    }

    public void AddPlayer(GamePlayer player)
    {
        if (totalPlayers == -1)
        {
            totalPlayers = player.totalPlayers;
        }

        gamePlayersList.Add(player);
    }

    public void RemovePlayer(GamePlayer player)
    {
        if(gamePlayersList.Contains(player))
        {
            gamePlayersList.Remove(player);
        }
    }

    [Server]
    public GamePlayer GetNextPlayer()
    {
        int n = currentTurnPlayer.playerID + 1;

        if (n > LobbyManager.Instance.maxPlayers - 1)
        {
            n = 0;
        }

        return gamePlayersList[n];
    }

    private IEnumerator SetupPlayerPositions()
    {
        while (localPlayer == null)
        {
            yield return null;
        }

        Debug.Log("Local player name: " + localPlayer.playerName);

        gamePlayersList = gamePlayersList.OrderBy(o => o.playerID).ToList();
        playerChairsList = playerChairsList.ShiftRight(localPlayer.playerID); //clockwise
        //playerChairsList = playerChairsList.ShiftLeft(localPlayer.playerID); //counter clockwise

        int length = gamePlayersList.Count;

        for (int i = 0; i < length; i++)
        {
            playerChairsList[i].PlacePlayer(gamePlayersList[i]);
        }
    }

    //server
    #region Server Game States

    [Server]
    private void ServerState_Reset()
    {
        isPlaying = false;
        readyHands = 0;
        currentTurnPlayer = currentWinningPlayer;
        previousPlayer = null;
        passTurnAmount = 0;
        rematchVotes = 0;
        firstTurn = false;

        ServerEnterGameState(GameState.Reset, "Shuffling");

        ServerNextState(StringEnum.GetStringValue(GameState.Assigning));
    }

    [Server]
    private void ServerState_AddBots()
    {
        int characterVariations = CharacterManager.Instance.characters.Count;
        for (int i = 0; i < botCounts; i++)
        {
            GameObject botGO = Instantiate(botPrefab as GameObject);
            botGO.GetComponent<BotPlayer>().botID = i;
            NetworkServer.Spawn(botGO);
            //NetworkServer.SpawnWithClientAuthority(botGO, gameObject);
        }

        ServerEnterGameState(GameState.AddingBots, "Adding bots");

        ServerNextState(StringEnum.GetStringValue(GameState.SetupPlayers));
    }

    [Server]
    private void ServerState_SetupPlayers()
    {
        ServerEnterGameState(GameState.SetupPlayers, "Setting up players");

        ServerNextState(StringEnum.GetStringValue(GameState.Assigning));
    }

    [Server]
    private void ServerState_AssignCards()
    {
        Debug.Log("Assigning cards");
        isPlaying = false;

        for (int i = 0; i < gamePlayersList.Count; i++)
        {
            CardData[] newCards = deck.GetNextCards(13);

            if(gamePlayersList[i] is HumanPlayer)
            {
                var addCardsMsg = new ServerAddCardsMsg();
                addCardsMsg.cardDatas = newCards;
                NetworkServer.SendToClientOfPlayer(gamePlayersList[i].gameObject, ServerAddCardsMsg.msgID, addCardsMsg);
            }
            else if(gamePlayersList[i] is BotPlayer)
            {
                gamePlayersList[i].Hand.UpdateCardDatas(newCards);
                ServerAddReadyHands();
            }
        }

        ServerEnterGameState(GameState.Assigning, "Assigning cards");
    }

    [Server]
    private void ServerState_DealCards()
    {
        isPlaying = false;
        ServerEnterGameState(GameState.Dealing, "Dealing cards");

        if(firstTurn)
        {
            ServerNextState(StringEnum.GetStringValue(GameState.FirstRound), 1.15f * gamePlayersList.Count);
        }
        else
        {
            ServerNextState(StringEnum.GetStringValue(GameState.NewRound), 1.15f * gamePlayersList.Count);
        }
    }

    [Server]
    private void ServerState_FirstRound()
    {
        isPlaying = true;

        int min = 0;
        int max = Enum.GetValues(typeof(Card.Suit)).Length * Enum.GetValues(typeof(Card.Rank)).Length; //impossibru tho

        currentTurnPlayer = null;

        while (currentTurnPlayer == null)
        {
            min++;
            for (int i = 0; i < gamePlayersList.Count; i++)
            {
                List<CardData> lcd = gamePlayersList[i].GetComponent<Hand>().cardDatas.ToList();
                if (lcd.Exists(c => c.value == min))
                {
                    CardData cd = lcd.Find(c => c.value == min);
                    currentPlayerIndex = i;
                    currentTurnPlayer = gamePlayersList[i];
                    break;
                }
            }
        }

        CmdUpdateLowestCard(min);

        ServerEnterGameState(GameState.FirstRound, "<color=magenta>"+currentTurnPlayer.playerName+ "</color>'s turn");

        if(currentTurnPlayer is HumanPlayer)
        {
            var msg = new ServerAssignTurnMsg();
            msg.isTurn = msg.firstTurn = true;
            NetworkServer.SendToClientOfPlayer(currentTurnPlayer.gameObject,ServerAssignTurnMsg.msgID,msg);
        }
        else if(currentTurnPlayer is BotPlayer)
        {
            currentTurnPlayer.AssignTurn(true, true);
        }
    }

    [Server]
    private void ServerState_GameRound()
    {
        isPlaying = true;

        if (currentTurnPlayer != null)
        {
            if (currentTurnPlayer is HumanPlayer)
            {
                var msg = new ServerAssignTurnMsg();
                msg.isTurn = true;
                msg.firstTurn = false;
                NetworkServer.SendToClientOfPlayer(currentTurnPlayer.gameObject, ServerAssignTurnMsg.msgID, msg);
            }
            else if (currentTurnPlayer is BotPlayer)
            {
                currentTurnPlayer.AssignTurn(true, false);
            }
        }

        ServerEnterGameState(GameState.GameRound, "<color=magenta>" + currentTurnPlayer.playerName + "</color>'s turn");
    }

    [Server]
    private void ServerState_NewRound()
    {
        isPlaying = true;

        passTurnAmount = 0;

        currentPlayedHand.Clear();
        currentTurnPlayer = currentWinningPlayer;
        currentPlayerIndex = gamePlayersList.IndexOf(currentTurnPlayer);

        ServerEnterGameState(GameState.NewRound, "<color=magenta>" + currentTurnPlayer.playerName + "</color>'s turn");

        if (currentTurnPlayer is HumanPlayer)
        {
            var msg = new ServerAssignTurnMsg();
            msg.isTurn = msg.firstTurn = true;
            NetworkServer.SendToClientOfPlayer(currentTurnPlayer.gameObject, ServerAssignTurnMsg.msgID, msg);
        }
        else if (currentTurnPlayer is BotPlayer)
        {
            currentTurnPlayer.AssignTurn(true, true);
        }
    }

    [Server]
    private void ServerState_GameOver()
    {
        isPlaying = false;

        Dictionary<GamePlayer, int> playerScoresDict = new Dictionary<GamePlayer, int>();

        foreach (GamePlayer player in gamePlayersList)
        {
            if(player != currentWinningPlayer)
            {
                playerScoresDict.Add(player, CalculatePlayerScore(player));
            }
        }

        var valueCollection = playerScoresDict.Values;

        int totalOtherScores = 0;

        foreach (int score in valueCollection)
        {
            totalOtherScores -= score;
        }

        playerScoresDict.Add(currentWinningPlayer, totalOtherScores);

        int length = gamePlayersList.Count;

        string[] playerNames = new string[length];
        int[] scores = new int[length];
        bool[] wins = new bool[length];

        for (int i = 0; i < length; i++)
        {
            GamePlayer player = gamePlayersList[i];
            playerNames[i] = player.playerName;
            scores[i] = playerScoresDict[player];
            wins[i] = player == currentWinningPlayer;
        }

        RpcShowResult(playerNames, scores, wins);
        RpcGameoverAnimation(currentWinningPlayer.playerID);

        /*if(currentWinningPlayer is BotPlayer)
        {
            var msg = new ServerDecideWinnerMsg();
            NetworkServer.SendToClientOfPlayer(currentWinningPlayer.gameObject, ServerDecideWinnerMsg.msgID, msg);
        }*/

        ServerEnterGameState(GameState.GameOver, "Game Over");
    }

    #endregion

    #region Server Functions

    [Server]
    void ServerEnterGameState(GameState newState, string message)
    {
        Debug.Log("Server Enter state:" + newState);
        gameState = newState;
        RpcGameState(newState, message);
    }

    [Server]
    private void ServerNextState(string functionName, float delay = 1f)
    {
        Debug.Log("Server next state:" + functionName);
        Invoke(functionName, delay);
    }

    [Server]
    private void ServerNextPlayer()
    {
        Debug.Log("ServerNextPlayer");

        if (currentTurnPlayer != null)
        {
            if (currentTurnPlayer is HumanPlayer)
            {
                var msg = new ServerAssignTurnMsg();
                msg.isTurn = msg.firstTurn = false;
                NetworkServer.SendToClientOfPlayer(currentTurnPlayer.gameObject, ServerAssignTurnMsg.msgID, msg);
            }
            else if (currentTurnPlayer is BotPlayer)
            {
                currentTurnPlayer.AssignTurn(false, false);
            }
        }
        /*else
        {
            currentPlayerIndex--;
        }*/

        currentTurnPlayer = null;

        while (currentTurnPlayer == null)
        {
            /*if(gamePlayersList.Count < 2)
            {
                break;
            }*/

            currentPlayerIndex++;

            if (currentPlayerIndex >= gamePlayersList.Count)
            {
                currentPlayerIndex = 0;
            }

            currentTurnPlayer = gamePlayersList[currentPlayerIndex];
            Debug.Log("current turn player name: " + currentTurnPlayer.playerName);
        }

        if(currentTurnPlayer == currentWinningPlayer)
        {
            passTurnAmount = 0;
            ServerNextState(StringEnum.GetStringValue(GameState.NewRound));
        }
        else
        {
            if(gameState == GameState.FirstRound && currentPlayedHand.Count < 1)
            {
                currentWinningPlayer = currentTurnPlayer;
                ServerNextState(StringEnum.GetStringValue(GameState.NewRound));
            }
            else
            {
                ServerNextState(StringEnum.GetStringValue(GameState.GameRound));
            }
        }
    }

    #endregion

    #region Client
    [Client]
    private void ClientHandleState(GameState newState, string message)
    {
        gameState = newState;
        string msg = "Client state:" + newState + " : " + message;

        //infoText.text = message;


        switch (newState)
        {
            case GameState.Reset:
                ClientState_Reset();
                break;

            case GameState.Dealing:
                ClientState_DealCards();
                break;

            case GameState.SetupPlayers:
                ClientState_SetupPlayers();
                break;

            case GameState.NewRound:
                ClientState_NewRound();
                break;

            case GameState.GameOver:
                ClientState_GameOver();
                break;
        }
    }

    [Client]
    private void ClientState_GameOver()
    {
        Deck.Instance.ThrowAwayStack();
        localPlayer.CmdChangeTurnState(GamePlayer.TurnState.Waiting);
    }

    [Client]
    private void ClientState_Reset()
    {
        uiManager.AnimateResultPanel(false);
        Deck.Instance.ResetDeck();

        for (int i = 0; i < gamePlayersList.Count; i++)
        {
            gamePlayersList[i].ResetPlayer();
        }
    }

    [Client]
    private void ClientState_SetupPlayers()
    {
        StartCoroutine(SetupPlayerPositions());
    }

    [Client]
    private void ClientState_DealCards()
    {
        Debug.Log("deal cards: "+gamePlayersList.Count);
        Deck.Instance.DealCards(gamePlayersList, localPlayer.playerID, false);
    }

    [Client]
    private void ClientState_NewRound()
    {
        Deck.Instance.ThrowAwayStack();
    }

    #endregion

    #region Rpc

    [ClientRpc]
    private void RpcGameState(GameState newState, string message)
    {
        ClientHandleState(newState, message);
    }

    [ClientRpc]
    private void RpcGreyoutStack()
    {
        Deck.Instance.GreyoutStack(true);
    }

    [ClientRpc]
    private void RpcShowResult(string[] playerNames, int[] scores, bool[] wins)
    {
        int length = playerNames.Length;

        for (int i = 0; i < length; i++)
        {
            uiManager.AddResultPlayer(playerNames[i], scores[i], wins[i]);
        }
        uiManager.AnimateResultPanel(true);
    }

    [ClientRpc]
    private void RpcGameoverAnimation(int winnerID)
    {
        foreach (GamePlayer gamePlayer in gamePlayersList)
        {
            if(gamePlayer.playerID == winnerID)
            {
                StartCoroutine(gamePlayer.SetEmotion(Character.Emotion.Happy, true));
            }
            else
            {
                StartCoroutine(gamePlayer.SetEmotion(Character.Emotion.Sad, true));
            }
        }
    }

    #endregion

    #region Commands

    [Command]
    private void CmdUpdateLowestCard(int value)
    {
        lowestCardValue = value;
    }

    #endregion

    private int CalculatePlayerScore(GamePlayer player)
    {
        int cardsLeft = player.Hand.cardDatas.Count;
        Debug.Log("cards left for player " + player.playerName + ": " + cardsLeft);

        if(cardsLeft == 0)
        {
            //winning player, no need calculate penalty
            return 0;
        }

        int score = cardsLeft;

        //calculate penalty for each remaining cards

        if(cardsLeft >= 8 && cardsLeft < 10)
        {
            score *= 2;
        }
        else if(cardsLeft >= 10 && cardsLeft < 13)
        {
            score *= 3;
        }
        else if(cardsLeft == 13)
        {
            score *= 4;
        }

        //calculate penalty for each two unused

        List<CardData> list =  player.Hand.cardDatas.ToDynList();

        int thresholdValue = (int)Card.Suit.Diamonds + (int)Card.Rank.Two * Enum.GetValues(typeof(Card.Suit)).Length;

        List<CardData> penaltyList = list.FindAll(cd => cd.value >= thresholdValue);

        for (int i = 0; i < penaltyList.Count; i++)
        {
            score *= 2;
        }

        return -score;
    }

    void OnDisable()
    {
        CancelInvoke();
    }
}
