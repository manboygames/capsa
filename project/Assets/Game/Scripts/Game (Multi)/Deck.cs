using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using CodeBureau;
using DG.Tweening;

public class Deck : MonoBehaviour {
    
    private static Deck instance;

    public static Deck Instance
    {
        get
        {
            return instance;
        }
    }

    public SpawningPool cardPool;

    private Dictionary<string, Sprite> spritesDict = new Dictionary<string, Sprite>();
    private string cardNamePrefix = "card";

    [HideInInspector]
    public List<Card> deckCardsList;

    private List<Card> cardsBackup; //for resetting deck
    private List<Card> stackCardsList = new List<Card>();

    private Dictionary<string, Card> cardsDict = new Dictionary<string, Card>();

    private bool isReady;

    public bool IsReady
    {
        get
        {
            return isReady;
        }
    }

    void Awake()
    {
        if (Deck.Instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;

        Sprite[] cardSprites = Resources.LoadAll<Sprite>("Sprites/Cards");

        foreach (Sprite sprite in cardSprites)
        {
            spritesDict.Add(sprite.name, sprite);
        }
    }

    void Start()
    {
        DOTween.Init();
        DOTween.defaultAutoPlay = AutoPlay.AutoPlayTweeners;
        StartCoroutine(InitCards());
    }

    IEnumerator InitCards()
    {
        while (cardPool.Units == null || cardPool.Units.Count == 0)
        {
            yield return null;
        }

        deckCardsList = new List<Card>();
        cardsBackup = new List<Card>();

        var suits = (int[])Enum.GetValues(typeof(Card.Suit));
        var ranks = (int[])Enum.GetValues(typeof(Card.Rank));

        for (int i = 0; i < suits.Length; i++)
        {
            for (int j = 0; j < ranks.Length; j++)
            {
                var card = cardPool.GetUnit().GetComponent<Card>();
                card.gameObject.SetActive(true);
                card.SetCard((Card.Suit)suits[i], (Card.Rank)ranks[j]);
                deckCardsList.Add(card);
                cardsDict.Add(card.name, card);
            }
        }

        cardsBackup = new List<Card>(deckCardsList);

        yield return new WaitForEndOfFrame();

        int length = deckCardsList.Count;

        for (int i = 0; i < length; i++)
        {
            deckCardsList[i].gameObject.SetActive(false);
            //cardPool.ReturnUnit(cards[i]);
        }

        isReady = true;
    }

    public void ResetDeck()
    {
        stackCardsList.Clear();

        deckCardsList = new List<Card>(cardsBackup);

        int length = deckCardsList.Count;

        for (int i = 0; i < length; i++)
        {
            Card card = deckCardsList[i];
            card.SetFace(false);
            card.GreyOut(false);
            cardPool.ReturnUnit(card.gameObject);
        }
    }

    public Sprite GetCardSprite(Card.Suit suit, Card.Rank rank)
    {
        return spritesDict[cardNamePrefix + Enum.GetName(typeof(Card.Suit), suit) + StringEnum.GetStringValue(rank)];
    }

    public Card GetCard(Card.Suit suit, Card.Rank rank)
    {
        Card card = cardsDict[cardNamePrefix + Enum.GetName(typeof(Card.Suit), suit) + Enum.GetName(typeof(Card.Rank), rank)];
        deckCardsList.Remove(card);
        return card;
    }

    public Card GetNextCard()
    {
        if (deckCardsList == null || deckCardsList.Count < 1)
        {
            return null;
        }

        int random = UnityEngine.Random.Range(0, deckCardsList.Count);
        Card card = deckCardsList[random];
        deckCardsList.RemoveAt(random);
        return card;
    }

    public CardData[] GetNextCards(int amount)
    {
        if (deckCardsList == null || deckCardsList.Count < amount)
        {
            return null;
        }

        CardData[] datas = new CardData[amount];

        for (int c = 0; c < amount; c++)
        {
            Card card = GetNextCard();
            if (card)
            {
                datas[c] = card.cardData;
            }
        }

        return datas;
    }

    public bool IsEmpty()
    {
        return deckCardsList.Count == 0;
    }

    public void DealCards(List<GamePlayer> players, int localPlayerID, bool animate)
    {
        Sequence sequence = DOTween.Sequence();

        int pLength = players.Count;
        int cLength = 13;

        int i = 0;

        for (int c = 0; c < cLength; c++)
        {
            for (int p = 0; p < pLength; p++)
            {
                try
                {
                    var hand = players[p].GetComponent<Hand>();
                    var datas = hand.cardDatas;
                    Card card = GetCard(datas[c].suit, datas[c].rank);
                    hand.AddPhysicalCard(card, p == localPlayerID, c);

                    float tPos = 0.05f * i;

                    //sequence.Insert(tPos, card.Move(new Vector3(0, 0, 0), 0.5f, true, Card.SFX.Place));
                    sequence.Insert(tPos, card.transform.DOMove(new Vector3(0, 0, 0), 0.5f).From());
                    sequence.Insert(tPos, card.transform.DORotate(hand.cardsContainer.rotation.eulerAngles, 0.5f, RotateMode.FastBeyond360));
                    sequence.InsertCallback(tPos, PlayPlaceSound);

                    i++;
                }
                catch
                {
                    Debug.Log("i error: " + i);
                }
                
            }
        }

        sequence.OnComplete(CardsDealedForPlayers);
        sequence.Play();
    }

    private void CardsDealedForPlayers()
    {
        
    }

    private void PlayPlaceSound()
    {
        AudioManager.Instance.PlaySFX("Place");
    }

    public void AddPlayedHandToStack(Card[] handCards)
    {
        foreach (Card c in handCards)
        {
            stackCardsList.Add(c);
        }
    }

    public void ThrowAwayStack()
    {
        if(stackCardsList.Count == 0)
        {
            return;
        }

        float duration = 0.2f;
        foreach (Card c in stackCardsList)
        {
            c.transform.DOMoveY(10, duration);
            c.transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-180f,180f)), duration, RotateMode.LocalAxisAdd);
        }
        AudioManager.Instance.PlaySFX("Shove");
    }

    public void GreyoutStack(bool greyout)
    {
        foreach (Card c in stackCardsList)
        {
            c.GreyOut(greyout);
        }
    }
}
