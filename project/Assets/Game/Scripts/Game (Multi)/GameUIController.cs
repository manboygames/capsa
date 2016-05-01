using UnityEngine;
using TextFx;
using UnityEngine.UI;
using DG.Tweening;
using CodeBureau;
using AdvancedInspector;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour {

    private static GameUIController instance;

    public static GameUIController Instance
    {
        get
        {
            return instance;
        }
    }

    public Camera uiCamera;
    private float pixelsPerUnit;

    [Inspect,Group("Player")]
    public CanvasGroup playerButtonsGroup;
    [Inspect, Group("Player")]
    public Button playButton;
    [Inspect, Group("Player")]
    public Button passButton;

    [Inspect, Group("Hand")]
    public TextFxUGUI textFx;
    [Inspect, Group("Hand")]
    public Image iconHand;

    [System.Serializable]
    public class HandIconDictionary : UDictionary<HandType, Sprite> { }

    public HandIconDictionary handIconDictionary = new HandIconDictionary();

    [System.Serializable]
    public class FlushIconDictionary : UDictionary<Card.Suit, Sprite> { }

    public FlushIconDictionary flushIconDictionary = new FlushIconDictionary();

    [Inspect, Group("Info")]
    public GameObject infoPanel;
    private CanvasGroup infoGroup;
    private Text infoText;

    [Inspect, Group("Result")]
    public CanvasGroup resultGroup;
    [Inspect, Group("Result")]
    public Text resultText;
    [Inspect, Group("Result")]
    public GameObject resultButtons;
    [Inspect, Group("Result")]
    public ResultPlayer resultPlayerPrefab;

    private List<ResultPlayer> resultPlayers = new List<ResultPlayer>();

    [Inspect, Group("Particles")]
    public ParticleSystem PsStarfall;
    [Inspect, Group("Particles")]
    public ParticleSystem PsFireworks;

    private Canvas canvas;

    void Awake()
    {
        infoText = infoPanel.GetComponentInChildren<Text>();
        infoGroup = infoPanel.GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
        pixelsPerUnit = GetComponent<Canvas>().referencePixelsPerUnit;
        instance = this;

        ShowResultPanel(false);
    }

    public void SetPlayerButtonsActive(bool isActive, bool firstTurn = false)
    {
        //playerButtonsGroup.Activate(isActive);
        playerButtonsGroup.interactable = isActive;
        passButton.interactable = !firstTurn;
    }

    public void PlayComboFX(HandType handType)
    {
        AudioManager.Instance.PlaySFXDelayed(0.2f, "Combo", (int)handType - 4);
        textFx.SetText(StringEnum.GetStringValue(handType));

        iconHand.color = new Color(iconHand.color.r, iconHand.color.g, iconHand.color.b, 0);
        Sequence iconSequence = DOTween.Sequence();
        iconSequence.Append(DOTween.ToAlpha(() => iconHand.color, x => iconHand.color = x, 1, 0.5f));
        iconSequence.AppendInterval(2.5f);
        iconSequence.Append(DOTween.ToAlpha(() => iconHand.color, x => iconHand.color = x, 0, 0.5f));
        iconSequence.Play();

        textFx.AnimationManager.PlayAnimation(() => { OnTextAnimated(handType); });
        if (handType == HandType.StraightFlush)
        {
            PsFireworks.Play();
        }

        AudioManager.Instance.PlaySFXDelayed(0.2f,"Combo", (int)handType - 4);

        SwitchComboIcon(handType);
    }

    public void PlayFlushFX(HandType handType, Card.Suit suit)
    {
        if(handType != HandType.Flush)
        {
            return;
        }

        PlayComboFX(handType);
        iconHand.sprite = flushIconDictionary[suit];
    }

    private void SwitchComboIcon(HandType handType)
    {
        if (handType == HandType.Flush)
        {
            return;
        }

        if (handType < HandType.Straight)
        {
            return;
        }

        iconHand.sprite = handIconDictionary[handType];
    }

    private void OnTextAnimated(HandType handType)
    {
        /*if(handType == HandType.StraightFlush)
        {
            PsFireworks.Play();
        }*/
    }

    public void ShowPlayerInfo(Vector3 pos3D, PlayerChair.Position chairPos, string message)
    {
        Debug.Log("ShowPlayerInfo");
        infoGroup.alpha = 1;

        RectTransform infoPanelRect = infoPanel.GetComponent<RectTransform>();

        /*Vector2 viewportPoint = uiCamera.WorldToViewportPoint(pos3D);
        Vector2 pos2D = new Vector2(
        ((viewportPoint.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
        ((viewportPoint.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));

        infoPanelRect.anchoredPosition = pos2D;*/

        GameUtils.SetPosition2D(pos3D, infoPanelRect, uiCamera, canvas);

        Vector3 rectPos = infoPanelRect.position;
        float move = 25f / pixelsPerUnit;

        float duration = 1f;

        switch (chairPos)
        {
            case PlayerChair.Position.West:
                infoPanelRect.pivot = new Vector2(0f, 0.5f);
                infoPanelRect.DOMoveX(rectPos.x + move, duration);
                break;
            case PlayerChair.Position.North:
                infoPanelRect.pivot = new Vector2(0.5f, 1f);
                infoPanelRect.DOMoveX(rectPos.x + move, duration);
                break;
            case PlayerChair.Position.East:
                infoPanelRect.pivot = new Vector2(1f, 0.5f);
                infoPanelRect.DOMoveX(rectPos.x - move, duration);
                break;
            case PlayerChair.Position.South:
                infoPanelRect.pivot = new Vector2(0.5f, 0f);
                infoPanelRect.DOMoveY(rectPos.y + move, duration);
                break;
        }

        infoGroup.DOFade(0, duration * 0.5f).SetDelay(1f);

        infoText.text = message;
    }

    #region Result

    public void AnimateResultPanel(bool show)
    {
        if (show)
        {
            ShowResultPanel(true);
            resultGroup.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.25f);
        }
        else
        {
            PsStarfall.Stop();
            resultGroup.transform.localScale = new Vector3(0, 0, 0);
            resultGroup.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.25f), 0.25f).OnComplete(ResetResultPanel);
        }
        
        AudioManager.Instance.PlaySFX("GameOver", 0);
    }

    private void ShowResultPanel(bool show)
    {
        resultGroup.alpha = show ? 1 : 0;
        resultGroup.interactable = show;
        resultGroup.blocksRaycasts = show;
    }

    private void ResetResultPanel()
    {
        foreach (ResultPlayer rp in resultPlayers)
        {
            Destroy(rp.gameObject);
        }

        resultPlayers.Clear();

        resultGroup.transform.localScale = new Vector3(1, 1, 1);
        ShowResultPanel(false);
    }

    public void AddResultPlayer(string playerName, int score, bool win)
    {
        ResultPlayer player = Instantiate(resultPlayerPrefab);
        resultPlayers.Add(player);
        player.transform.SetParent(resultGroup.transform,false);
        //player.transform.SetSiblingIndex(transform.childCount - 2);
        player.SetPlayer(playerName, score, win);
        resultButtons.transform.SetAsLastSibling();
    }

    public void DecideWinner()
    {
        resultText.text = "You Win !";
        PsStarfall.Play();
        AudioManager.Instance.PlaySFX("GameOver", "SFX_Yahoo");
    }

    public void Rematch(bool isRematch)
    {
        resultGroup.interactable = false;

        var msg = new ClientRematchMessage();
        msg.rematch = isRematch;
        LobbyManager.Instance.client.Send(ClientRematchMessage.msgID, msg);
    }


    #endregion

}
