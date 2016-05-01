using UnityEngine;
using System.Collections;
using System;
using CodeBureau;
using UnityEngine.Networking;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(Sprite))]
[RequireComponent(typeof(BoxCollider2D))]
public class Card : MonoBehaviour {

    public enum Suit
    {
        Diamonds = 1,
        Clubs = 2,
        Hearts = 3,
        Spades = 4
    }

    public enum Rank
    {
        [StringValue("3")]
        Three = 1,
        [StringValue("4")]
        Four = 2,
        [StringValue("5")]
        Five = 3,
        [StringValue("6")]
        Six = 4,
        [StringValue("7")]
        Seven = 5,
        [StringValue("8")]
        Eight = 6,
        [StringValue("9")]
        Nine = 7,
        [StringValue("10")]
        Ten = 8,
        [StringValue("J")]
        Jack = 9,
        [StringValue("Q")]
        Queen = 10,
        [StringValue("K")]
        King = 11,
        [StringValue("A")]
        Ace = 12,
        [StringValue("2")]
        Two = 13
    }

    public CardData cardData;

    private SpriteRenderer spriteRenderer;
    private Sprite faceSprite;
    private Sprite backSprite;
    private BoxCollider2D boxCollider;

    [System.Serializable]
    public class OnCardClicked : UnityEvent<Card> { }

    [HideInInspector]
    public OnCardClicked onCardClicked = new OnCardClicked();

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        backSprite = spriteRenderer.sprite;
        boxCollider = GetComponent<BoxCollider2D>();
        SetInteractable(false);
    }

    IEnumerator Start()
    {
        while (Deck.Instance == null)
        {
            yield return null;
        }

        spriteRenderer.sprite = Deck.Instance.GetCardSprite(cardData.suit, cardData.rank);
        SetSprite(cardData.suit, cardData.rank, cardData.isShown);
    }

    /*void OnMouseDown()
    {
        onCardClicked.Invoke(this);
    }*/

    public void SetCard(Suit suit, Rank value)
    {
        cardData = new CardData(suit, value);
        gameObject.name = "card"+Enum.GetName(typeof(Suit), suit) + Enum.GetName(typeof(Rank),value);
        SetSprite(suit, value);
    }

    public void SetCard(CardData data)
    {
        cardData = new CardData(data.suit, data.rank);
        gameObject.name = "card" + Enum.GetName(typeof(Suit), data.suit) + Enum.GetName(typeof(Rank), data.rank);
        SetSprite(data.suit, data.rank);
    }

    private void SetSprite(Suit suit, Rank value, bool shown = true)
    {
        faceSprite = Deck.Instance.GetCardSprite(suit, value);

        if(shown)
        {
            spriteRenderer.sprite = faceSprite;
        }
        else
        {
            spriteRenderer.sprite = backSprite;
        }
    }

    public void SetFace(bool shown)
    {
        transform.localEulerAngles = new Vector3(0, 0, 0);
        cardData.isShown = shown;
        if(shown)
        {
            spriteRenderer.sprite = faceSprite;
        }
        else
        {
            spriteRenderer.sprite = backSprite;
        }
    }

    public void SetInteractable(bool interactable)
    {
        boxCollider.enabled = interactable;
    }

    public void Flip()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Insert(0,transform.DOLocalRotate(new Vector3(180, 180), 0.3f, RotateMode.LocalAxisAdd));
        sequence.InsertCallback(0.15f, () => SetFace(!cardData.isShown));
        sequence.Play();
    }

    public void GreyOut(bool greyout)
    {
        if(greyout)
        {
            spriteRenderer.color = Color.grey;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    /*public void Glow(bool isOn)
    {
        Debug.Log("Card glow");
        if(isOn)
        {
            highlighter.FlashingOn(Color.white, Color.yellow);
        }
        else
        {
            highlighter.FlashingOff();
        }
    }*/

}

[Serializable]
public struct CardData
{
    public Card.Suit suit;
    public Card.Rank rank;
    public int value;
    public bool isShown;
    
    public CardData(Card.Suit suit, Card.Rank rank, bool shown = true)
    {
        this.suit = suit;
        this.rank = rank;
        value = (int)suit + (int)rank * Enum.GetValues(typeof(Card.Suit)).Length;
        isShown = shown;
    }
}

public class SyncListCardData : SyncListStruct<CardData>
{

}
