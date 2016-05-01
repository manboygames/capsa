using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class HumanPlayer : GamePlayer {

    protected override IEnumerator Start()
    {
        yield return base.Start();

        if (isLocalPlayer)
        {
            gameManager.SetLocalPlayer(this);

            if (isServer)
            {
                gameManager.ServerAddConnection(connectionToClient.connectionId);
            }
            else
            {
                var msg = new ClientPlayerReadyMsg();
                client.Send(ClientPlayerReadyMsg.msgID, msg);
            }

            uiManager.playButton.onClick.AddListener(OnPlayButtonClick);
            uiManager.passButton.onClick.AddListener(OnPassButtonClick);

            emojiController.onEmojiClicked.AddListener(OnEmojiClick);
            chatController.onChatSubmit.AddListener(OnChatSubmit); ;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CmdChangeName(CharacterManager.Instance.characterName);
        CmdChangeCharacterID(CharacterManager.Instance.characterID);

        client.RegisterHandler(ServerDecideWinnerMsg.msgID, OnPlayerWin);
        client.RegisterHandler(ServerAddCardsMsg.msgID, OnAddCards);
        client.RegisterHandler(ServerAssignTurnMsg.msgID, OnAssignTurn);
    }

    public void OnAddCards(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<ServerAddCardsMsg>();
        hand.CmdUpdateCardDatas(msg.cardDatas);

        if (isServer)
        {
            GameManager.Instance.ServerAddReadyHands();
        }
        else
        {
            var readyMsg = new ClientPlayerReadyMsg();
            client.Send(ClientPlayerReadyMsg.msgID, readyMsg);
        }
    }

    private void OnAssignTurn(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<ServerAssignTurnMsg>();
        AssignTurn(msg.isTurn, msg.firstTurn);
        uiManager.SetPlayerButtonsActive(msg.isTurn, msg.firstTurn);
    }

    private void OnPlayButtonClick()
    {
        switch (gameManager.gameState)
        {
            case GameState.FirstRound:
                PlayHand(true, true);
                break;
            case GameState.NewRound:
                PlayHand(true, false);
                break;
            case GameState.GameRound:
                PlayHand(false, false);
                break;
        }
    }

    protected override HandType PlayHand(bool newRound, bool requireLowestCard)
    {
        HandType handType = base.PlayHand(newRound, requireLowestCard);

        if(handType == HandType.Invalid)
        {
            //dont play if hand invalid
            return HandType.Invalid;
        }

        uiManager.SetPlayerButtonsActive(false, newRound);

        var msg = new ClientPlayHandMsg();
        msg.cardDatas = hand.PlayedHandData.ToArray();
        msg.handType = handType;
        msg.remainingCards = hand.RemainingCards;
        msg.playingSet = hand.PlayedHandData.Count == 5; //if hand is a set
        client.Send(ClientPlayHandMsg.msgID, msg);

        return handType;
    }

    private void OnPassButtonClick()
    {
        if (gameManager.gameState == GameState.FirstRound || gameManager.gameState == GameState.NewRound)
        {
            return;
        }

        if (gameManager.gameState == GameState.GameRound)
        {
            PassTurn();
        }
    }

    protected override void PassTurn()
    {
        uiManager.SetPlayerButtonsActive(false);
        base.PassTurn();
    }

    private void OnEmojiClick(Sprite emojiSprite)
    {
        var msg = new ClientPlayEmojiMsg();
        msg.spriteName = emojiSprite.name;
        client.Send(ClientPlayEmojiMsg.msgID, msg);
    }

    private void OnChatSubmit(string chatMsg)
    {
        var msg = new ClientSubmitChatMsg();
        msg.chatMsg = chatMsg;
        client.Send(ClientSubmitChatMsg.msgID, msg);
    }

    private void OnPlayerWin(NetworkMessage netMsg)
    {
        //var msg = netMsg.ReadMessage<ServerDecideWinnerMsg>();
        uiManager.DecideWinner();
    }

    void OnDisable()
    {
        uiManager.playButton.onClick.RemoveListener(OnPlayButtonClick);
        uiManager.passButton.onClick.RemoveListener(OnPassButtonClick);

        emojiController.onEmojiClicked.RemoveListener(OnEmojiClick);
        chatController.onChatSubmit.RemoveListener(OnChatSubmit);

        client.UnregisterHandler(ServerDecideWinnerMsg.msgID);
        client.UnregisterHandler(ServerAddCardsMsg.msgID);
        client.UnregisterHandler(ServerAssignTurnMsg.msgID);
    }

}
