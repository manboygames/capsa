using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class Hand : NetworkBehaviour {

    public Transform cardsContainer;
    public float cardDistance = 0.4f;
    
    public SyncListCardData cardDatas = new SyncListCardData();

    private NetworkClient client;

    private List<Card> handCards = new List<Card>();

    private List<Card> playedHand = new List<Card>();

    private GamePlayer player;

    public List<CardData> PlayedHandData
    {
        get
        {
            List<CardData> cardsData = new List<CardData>();

            foreach (Card c in playedHand)
            {
                cardsData.Add(c.cardData);
            }

            return cardsData;
        }
    }

    private int remainingCards;

    public int RemainingCards
    {
        get
        {
            return remainingCards;
        }

        set
        {
            remainingCards = value;
        }
    }

    void Awake()
    {
        player = GetComponent<GamePlayer>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        cardDatas.Callback = OnCardDatasUpdated;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    void Update()
    {
        if(!hasAuthority)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit;
            List<RaycastHit2D> hits;

            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 10);
            hits = new List<RaycastHit2D>(Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 10, 1 << LayerMask.NameToLayer("Cards")));

            if(hits.Count > 0)
            {
                hits = hits.OrderByDescending(c => c.collider.GetComponent<SpriteRenderer>().sortingOrder).ToList();

                Card card = hits[0].collider.GetComponent<Card>();

                if(handCards.Contains(card))
                {
                    OnCardClicked(card);
                }
            }
        }
    }

    public void SetCardsInteractable(bool interactable)
    {
        foreach (Card c in handCards)
        {
            c.SetInteractable(interactable);
        }
    }

    [Command]
    public void CmdUpdateCardDatas(CardData[] datas)
    {
        UpdateCardDatas(datas);
    }

    public void UpdateCardDatas(CardData[] datas)
    {
        cardDatas.Clear();

        List<CardData> dummyHand = datas.ToList();
        dummyHand = dummyHand.SortBySuits(true);
        dummyHand = dummyHand.SortByRank(true);

        foreach (CardData cd in dummyHand)
        {
            cardDatas.Add(cd);
        }
    }

    [Server]
    public void AssignCardDatas(CardData[] datas)
    {
        cardDatas.Clear();

        List<CardData> dummyHand = datas.ToList();
        dummyHand = dummyHand.SortBySuits(true);
        dummyHand = dummyHand.SortByRank(true);

        foreach (CardData cd in dummyHand)
        {
            cardDatas.Add(cd);
        }
    }

    [Command]
    public void CmdRemoveCardDatas(CardData[] datas)
    {
        foreach (CardData cardData in datas)
        {
            bool removed = cardDatas.Remove(cardData);

            if(!removed)
            {
                CardData firstFound = cardDatas.FirstOrDefault(cd => cd.value == cardData.value);
                cardDatas.Remove(firstFound);
            }
        }
    }

    public void RemoveCardDatas(List<CardData> datas)
    {
        foreach (CardData cardData in datas)
        {
            bool removed = cardDatas.Remove(cardData);

            if (!removed)
            {
                CardData firstFound = cardDatas.FirstOrDefault(cd => cd.value == cardData.value);
                cardDatas.Remove(firstFound);
            }
        }
    }

    [Client]
    private void OnCardDatasUpdated(SyncList<CardData>.Operation op, int itemIndex)
    {
        //Debug.Log("SyncListCardData: "+op+", count: "+ cardDatas.Count);

        if (cardDatas.Count == 13)
        {
            remainingCards = 13;
        }
    }

    public void AddPhysicalCard(Card card, bool shown, int index)
    {
        card.gameObject.SetActive(true);
        card.transform.SetParent(cardsContainer);
        card.transform.localPosition = new Vector3(-index * cardDistance, 0, 0);

        if (isLocalPlayer)
        {
            card.SetInteractable(true);
        }

        card.SetFace(shown);
        //card.SetFace(true); //for debugging
        card.GetComponent<SpriteRenderer>().sortingOrder = cardDatas.Count - index;

        handCards.Add(card);
    }

    private void OnCardClicked(Card card)
    {
        if (!playedHand.Contains(card))
        {
            if (playedHand.Count < 5)
            {
                AddCardToPlayedHand(card);
            }
        }
        else
        {
            RemoveCardFromPlayedHand(card);
        }
    }

    private void AddCardToPlayedHand(Card card)
    {
        playedHand.Add(card);
        card.transform.DOLocalMoveY(0.2f, 0.1f);
    }

    private void RemoveCardFromPlayedHand(Card card)
    {
        playedHand.Remove(card);
        card.transform.DOLocalMoveY(0, 0.1f);
    }

    public void ReturnPlayedHand()
    {
        foreach (Card card in playedHand)
        {
            RemoveCardFromPlayedHand(card);
        }
    }

    private void SortRemainingCards()
    {
        int length = handCards.Count;

        for (int i = 0; i < length; i++)
        {
            handCards[i].transform.DOLocalMoveX(-i * cardDistance,0.2f);
        }
    }

    public void ResetHand()
    {
        handCards.Clear();
        //cardDatas.Clear();
        playedHand.Clear();
    }

    [ClientRpc]
    public void RpcMoveCardsToPlay(CardData[] datas, HandType handType)
    {
        //move to middle
        int length = datas.Length;

        float cardsLength = cardDistance * length;
        float startPos = -cardsLength * 0.5f;
        //Vector3 randomVariation = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), 0);
        float duration = 0.2f;

        Card[] stackCards = new Card[length];

        for (int i = 0; i < length; i++)
        {
            try
            {
                Card card = handCards.Find(c => c.cardData.value == datas[i].value);

                PlayerChair.Position chairPos = player.chair.GetChairPosition();

                if (chairPos == PlayerChair.Position.West || chairPos == PlayerChair.Position.East)
                {
                    card.transform.DOMove(new Vector3(0, startPos + i * cardDistance, 0), duration);
                }
                else
                {
                    card.transform.DOMove(new Vector3(startPos + i * cardDistance, 0, 0), duration);
                }

                card.GetComponent<SpriteRenderer>().sortingOrder = GameManager.Instance.stackSortingOrder++;

                if (!isLocalPlayer)
                {
                    if (!card.cardData.isShown)
                    {
                        card.Flip();
                    }
                }

                stackCards[i] = card;

                handCards.Remove(card);

                bool removed = cardDatas.Remove(card.cardData);

                if (!removed)
                {
                    CardData firstFound = cardDatas.FirstOrDefault(cd => cd.value == card.cardData.value);
                    cardDatas.Remove(firstFound);
                }
            }
            catch (Exception)
            {
                Debug.Log("exception card is: " + datas[i].rank +" of "+datas[i].suit);
                throw;
            }
        }

        AudioManager.Instance.PlaySFX("Place");

        if (length == 5)
        {
            if (handType != HandType.Invalid)
            {
                if(handType == HandType.Flush)
                {
                    StartCoroutine(ShowFlushFX(handType, datas[2].suit, duration));
                }
                else
                {
                    StartCoroutine(ShowHandFX(handType, duration));
                }
            }
        }

        playedHand.Clear();
        Deck.Instance.AddPlayedHandToStack(stackCards);
        SortRemainingCards();
    }

    private IEnumerator ShowHandFX(HandType handType, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameUIController.Instance.PlayComboFX(handType);
    }

    private IEnumerator ShowFlushFX(HandType handType, Card.Suit suit, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameUIController.Instance.PlayFlushFX(handType, suit);
    }

    private IEnumerator ShowRemainingCards(int remainingCards, float delay)
    {
        yield return new WaitForSeconds(delay);

        string msg = "";

        if(remainingCards == 2)
        {
            msg = "<color=" + Color.yellow + ">2</color> cards left";
        }
        else if(remainingCards == 1)
        {
            msg = "<color=" + Color.red + ">1</color> card left";
        }

        GameUIController.Instance.ShowPlayerInfo(player.chair.indicator.position, player.chair.GetChairPosition(), msg);
    }
}
