using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerChair : MonoBehaviour {
    
    public enum Position
    {
        South = 0,
        West = 1,
        North = 2,
        East = 3
    }

    public Canvas canvas;
    public Camera uiCamera;
    public Transform bubbleContainer;

    public Transform playerFrameContainer;
    public Transform playerHandContainer;
    public Transform indicator;
    public List<PlayerChair> list;

    public EmojiBubble emojiBubble;
    public ChatBubble chatBubble;
    public Bubble.Direction bubbleDirection;
    public Transform bubbleSpawn;

    private AnchorPosition anchorPosition;

    void Awake()
    {
        anchorPosition = GetComponent<AnchorPosition>();
        anchorPosition.onPositionUpdated.AddListener(SetBubblePosition);
    }

    private void SetBubblePosition()
    {
        GameUtils.SetPosition2D(bubbleSpawn.position, emojiBubble.GetComponent<RectTransform>(), uiCamera, canvas);
        emojiBubble.SetDirection(bubbleDirection);
        GameUtils.SetPosition2D(bubbleSpawn.position, chatBubble.GetComponent<RectTransform>(), uiCamera, canvas);
        chatBubble.SetDirection(bubbleDirection);
    }

    public void PlacePlayer(GamePlayer player)
    {
        player.chair = this;
        player.transform.SetParent(playerFrameContainer, false);
        player.transform.localPosition = new Vector3(0, 0, 0);
        player.Hand.cardsContainer = playerHandContainer;

        player.emojiBubble = emojiBubble;
        player.chatBubble = chatBubble;
    }

    public Position GetChairPosition()
    {
        return (Position)list.IndexOf(this);
    }
}
