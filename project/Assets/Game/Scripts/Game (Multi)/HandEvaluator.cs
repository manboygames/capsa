using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CodeBureau;

public enum HandType
{
    Invalid = 0,
    Single = 1,
    Pair = 2,
    Triple = 3,
    [StringValue("Straight")]
    Straight = 4,
    [StringValue("Flush")]
    Flush = 5,
    [StringValue("Full House")]
    FullHouse = 6,
    [StringValue("Four of a Kind")]
    FourOfAKind = 7,
    [StringValue("Straight Flush")]
    StraightFlush = 8,
    [StringValue("Dragon")]
    Dragon = 9
}

public static class HandEvaluator
{

    public static HandType EvaluateHand(List<CardData> hand)
    {
        int length = hand.Count;

        if(length == 0 || length > 5)
        {
            return HandType.Invalid;
        }

        if(hand.IsStraightFlush())
        {
            return HandType.StraightFlush;
        }
        else if(hand.IsFourKind())
        {
            return HandType.FourOfAKind;
        }
        else if(hand.IsFullHouse())
        {
            return HandType.FullHouse;
        }
        else if(hand.IsFlush())
        {
            return HandType.Flush;
        }
        else if(hand.IsStraight())
        {
            return HandType.Straight;
        }
        else if(hand.IsTriple())
        {
            return HandType.Triple;
        }
        else if(hand.IsPair())
        {
            return HandType.Pair;
        }
        else if(hand.IsSingle())
        {
            return HandType.Single;
        }

        return HandType.Invalid;
    }

    public static bool CompareHand(List<CardData> handA, List<CardData> handB)
    {
        //first compare stronger set
        HandType ht1 = EvaluateHand(handA);
        HandType ht2 = EvaluateHand(handB);

        if(ht1 > ht2) //win, stronger set
        {
            return true;
        }
        else if(ht1 < ht2) //lose, weaker set
        {
            return false;
        }
        else if(ht1 == ht2)//draw, same set, compare value and conditions
        {
            switch(ht1)
            {
                case HandType.Single:
                case HandType.Pair:
                case HandType.Triple:
                    return CompareValue(handA, handB);
                //case HandType.Dragon:
                case HandType.StraightFlush:
                case HandType.Straight:
                    return CompareStraight(handA, handB);
                case HandType.Flush:
                    return CompareFlush(handA, handB);
                case HandType.FullHouse:
                case HandType.FourOfAKind:
                    return CompareMiddle(handA, handB);
            }
        }

        Debug.LogWarning("Both hands are invalid");
        return false;
    }

    public static bool CompareHand(CardData[] handA, CardData[] handB)
    {
        return CompareHand(handA.ToList(), handB.ToList());
    }

    public static bool CompareValue(List<CardData> handA, List<CardData> handB)
    {
        return handA.GetValue() > handB.GetValue();
    }

    public static bool CompareValue(CardData cardA, CardData cardB)
    {
        return cardA.value > cardB.value;
    }

    private static int GetValue(this List<CardData> hand)
    {
        return hand.Max(cd => cd.value);
    }

    public static bool CompareStraight(List<CardData> handA, List<CardData> handB)
    {
        List<CardData> newHandA = handA.SortByRank();
        List<CardData> newHandB = handB.SortByRank();

        int maxIndex = handA.Count - 1;

        //compare highest card, e.g: 3,4,5,7,A > 8,9,J,Q,K
        if (newHandA[maxIndex].rank > newHandB[maxIndex].rank)
        {
            return true;
        }
        else if(newHandA[maxIndex].rank < newHandB[maxIndex].rank)
        {
            return false;
        }
        else
        {
            //if same rank, compare which suit of high card is stronger
            return newHandA[maxIndex].suit > newHandB[maxIndex].suit;
        }
    }

    public static bool CompareFlush(List<CardData> handA, List<CardData> handB)
    {
        handA = handA.SortByRank();
        handB = handB.SortByRank();

        if(handA[0].suit == handB[0].suit)
        {
            return handA[4].rank > handB[4].rank;
        }
        else
        {
            return handA[0].suit > handB[0].suit;
        }
    }

    public static bool CompareMiddle(List<CardData> handA, List<CardData> handB)
    {
        List<CardData> newHandA = handA.SortByRank();
        List<CardData> newHandB = handB.SortByRank();

        //compare highest rank of triple, which is always the middle card after sorted 3,3,3,J,J | 7,7,Q,Q,Q
        //compare highest rank of quad, which is always the middle card after sorted 4,4,4,4,6 | 5,K,K,K,K
        if (newHandA[2].rank > newHandB[2].rank)
        {
            return true;
        }
        else if (newHandA[2].rank < newHandB[2].rank)
        {
            return false;
        }
        else
        {
            //if same rank, compare which suit of high card is stronger
            return newHandA[2].suit > newHandB[2].suit;
        }
    }

    #region Extensions

    public static bool ContainsLowestCard(this List<CardData> hand, int lowest = 1) //default: three diamonds, value = 1
    {
        Debug.Log("Contains lowest card?: " + lowest);
        return hand.Exists(c => c.value == lowest);
    }

    public static CardData GetLowestCard(this List<CardData> hand)
    {
        int lowest = hand.Min(c => c.value);
        return hand.Find(c => c.value == lowest);
    }

    public static bool IsSingle(this List<CardData> hand)
    {
        return hand.Count == 1;
    }

    public static bool IsPair(this List<CardData> hand)
    {
        if (hand.Count != 2)
        {
            return false;
        }

        return hand[0].rank == hand[1].rank;
    }

    public static bool IsTriple(this List<CardData> hand)
    {
        if (hand.Count != 3)
        {
            return false;
        }

        if (hand[0].rank == hand[1].rank && hand[1].rank == hand[2].rank)
        {
            return true;
        }

        return false;
    }

    public static bool IsStraight(this List<CardData> hand)
    {
        if (hand.Count != 5)
        {
            return false;
        }

        List<CardData> newHand = hand.SortByRank();

        if (newHand[4].rank == Card.Rank.Two) //if two is highest rank
        {
            if(newHand[3].rank == Card.Rank.Ace) //if second highest is ace
            {
                //High Two with Ace: J,Q,K,A,2
                bool highTwoWithAce = newHand[0].rank == Card.Rank.Jack
                    && newHand[1].rank == Card.Rank.Queen
                    && newHand[2].rank == Card.Rank.King;
                //Low Two with Ace: 3,4,5,A,2 -> A,2,3,4,5
                bool lowTwoWithAce = newHand[0].rank == Card.Rank.Three
                    && newHand[1].rank == Card.Rank.Four
                    && newHand[2].rank == Card.Rank.Five; 
                return highTwoWithAce || lowTwoWithAce;
            }
            else //if ace doesn't exist
            {
                return newHand[0].rank == Card.Rank.Three
                    && newHand[1].rank == Card.Rank.Four
                    && newHand[2].rank == Card.Rank.Five
                    && newHand[3].rank == Card.Rank.Six; //Low Two without Ace: 3,4,5,6,2 -> 2,3,4,5,6
            }
        }
        else if (newHand[4].rank == Card.Rank.Ace) //if ace is highest rank
        {
            return newHand[0].rank == Card.Rank.Ten
                && newHand[1].rank == Card.Rank.Jack
                && newHand[2].rank == Card.Rank.Queen
                && newHand[3].rank == Card.Rank.King; //High Ace: 10,J,Q,K,A
        }
        else //normal straight without circling back to lowest value
        {
            for (int i = 1; i <= 4; i++)
            {
                if (newHand[i].rank != (newHand[i - 1].rank + 1))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool IsFlush(this List<CardData> hand)
    {
        if (hand.Count != 5)
        {
            return false;
        }

        List<CardData> h = hand.SortBySuits();

        return h[0].suit == h[4].suit;
    }

    public static bool IsFullHouse(this List<CardData> hand)
    {
        if (hand.Count != 5)
        {
            return false;
        }

        List<CardData> newHand = hand.SortByRank();

        // xxxyy
        bool lowHouse = CheckTriple(newHand[0], newHand[1], newHand[2]) && CheckPair(newHand[3], newHand[4]);
        // xxyyy
        bool highHouse = CheckPair(newHand[0], newHand[1]) && CheckTriple(newHand[2], newHand[3], newHand[4]);

        return lowHouse || highHouse;
    }

    public static bool IsFourKind(this List<CardData> hand)
    {
        if(hand.Count != 5)
        {
            return false;
        }

        List<CardData> newHand = hand.SortByRank();

        bool lowFour = true; //xxxxy
        bool highFour = true; //xyyyy

        //check lowfour
        for (int i = 1; i <= 3; i++)
        {
            if(newHand[0].rank != newHand[i].rank)
            {
                lowFour = false;
                break;
            }
        }

        //check highFour
        for (int i = 3; i >= 1; i--)
        {
            if(newHand[4].rank != newHand[i].rank)
            {
                highFour = false;
                break;
            }
        }

        return lowFour || highFour;
    }

    public static bool IsStraightFlush(this List<CardData> hand)
    {
        return hand.IsStraight() && hand.IsFlush();
    }

    #endregion

    #region Helper
    public static List<CardData> SortBySuits(this List<CardData> hand, bool isReverse = false)
    {
        if(isReverse)
        {
            return hand.OrderByDescending(c => c.suit).ToList();
        }
        else
        {
            return hand.OrderBy(c => c.suit).ToList();
        }
    }

    public static List<CardData> SortByRank(this List<CardData> hand, bool isReverse = false)
    {
        if (isReverse)
        {
            return hand.OrderByDescending(c => c.rank).ToList();
        }
        else
        {
            return hand.OrderBy(c => c.rank).ToList();
        }
    }

    private static bool CheckPair(CardData a, CardData b)
    {
        return a.rank == b.rank;
    }

    private static bool CheckTriple(CardData a, CardData b, CardData c)
    {
        return a.rank == b.rank && b.rank == c.rank;
    }
    #endregion
}

