using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facet.Combinatorics;
using CodeBureau;
using System;

public class AIHandCheck : MonoBehaviour {

    [SerializeField]
    private List<CardData> playerCards = new List<CardData>();
    public Transform cardsContainer;
    
    public List<CardData> gameCards = new List<CardData>();

    private float cardDistance = 0.4f;

    IEnumerator Start()
    {
        while(Deck.Instance == null)
        {
            yield return null;
        }

        while(!Deck.Instance.IsReady)
        {
            yield return null;
        }

        playerCards = new List<CardData>();
        /*playerCards.Add(Deck.Instance.GetCard(Card.Suit.Diamonds, Card.Rank.Three).cardData);
        playerCards.Add(Deck.Instance.GetCard(Card.Suit.Clubs, Card.Rank.Three).cardData);
        playerCards.Add(Deck.Instance.GetCard(Card.Suit.Spades, Card.Rank.Three).cardData);
        playerCards.Add(Deck.Instance.GetCard(Card.Suit.Hearts, Card.Rank.Three).cardData);*/
        playerCards = playerCards.Union(Deck.Instance.GetNextCards(13-playerCards.Count).ToList()).ToList();
        playerCards = playerCards.SortBySuits(true);
        playerCards = playerCards.SortByRank(true);

        int length = playerCards.Count;

        for (int i = 0; i < length; i++)
        {
            Card card = Deck.Instance.GetCard(playerCards[i].suit, playerCards[i].rank);
            card.gameObject.SetActive(true);
            card.transform.SetParent(cardsContainer);
            card.transform.localPosition = new Vector3(-i * cardDistance, 0, 0);
        }
    }


    #region Counter

    public void CounterPlay()
    {
        List<CardData> botHand = BotCounterPlays();

        if (botHand == null)
        {
            Debug.Log("Bot is so weak");
        }
        else
        {
            botHand = botHand.SortBySuits();
            botHand = botHand.SortByRank();

            string s = string.Empty;

            foreach (CardData cd in botHand)
            {
                s += cd.rank + " of " + cd.suit + "\t";
            }
            Debug.Log(s);
        }
    }

    private List<CardData> BotCounterPlays()
    {
        switch (gameCards.Count)
        {
            case 1:
                return GetSingle(gameCards);

            case 2:
                return GetPair(gameCards);

            case 3:
                return GetTriple(gameCards);

            case 5:
                return GetFiver(gameCards);
        }

        return null;
    }

    private List<CardData> GetSingle(List<CardData> gameHand, bool useStrongest = false)
    {
        List<CardData> sortedHand = playerCards.SortBySuits(useStrongest);
        sortedHand = sortedHand.SortByRank(useStrongest);

        int count = sortedHand.Count;

        if (useStrongest)
        {
            if (gameHand == null || HandEvaluator.CompareValue(sortedHand[0], gameHand[0]))
            {
                return new List<CardData> { sortedHand[0] };
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (gameHand == null || HandEvaluator.CompareValue(sortedHand[i], gameHand[0]))
            {
                return new List<CardData> { sortedHand[i] };
            }
        }

        return null;
    }

    private List<CardData> GetPair(List<CardData> gameHand, bool useStrongest = false)
    {
        var groupedList = playerCards.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });

        if (useStrongest)
        {
            groupedList = groupedList.OrderByDescending(c => c.key);
        }
        else
        {
            groupedList = groupedList.OrderBy(c => c.key);
        }

        List<List<CardData>> pairs = new List<List<CardData>>();

        foreach (var group in groupedList)
        {
            if (group.list.Count >= 2)
            {
                group.list.RemoveRange(2, group.list.Count - 2);
                pairs.Add(group.list);
            }
        }

        if (useStrongest)
        {
            pairs = pairs.OrderByDescending(p => p[0].rank).ToList();
        }
        else
        {
            pairs = pairs.OrderBy(p => p[0].rank).ToList();
        }

        foreach (List<CardData> pair in pairs)
        {
            if (gameHand == null || HandEvaluator.CompareHand(pair, gameHand))
            {
                return pair;
            }
        }

        return null;
    }

    private List<CardData> GetTriple(List<CardData> gameHand, bool useStrongest = false)
    {
        var groupedList = playerCards.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });

        if (useStrongest)
        {
            groupedList = groupedList.OrderByDescending(c => c.key);
        }
        else
        {
            groupedList = groupedList.OrderBy(c => c.key);
        }

        List<List<CardData>> triples = new List<List<CardData>>();

        foreach (var group in groupedList)
        {
            if (group.list.Count >= 3)
            {
                group.list.RemoveRange(3, group.list.Count - 3);
                triples.Add(group.list);
            }
        }

        if (useStrongest)
        {
            triples = triples.OrderByDescending(p => p[0].rank).ToList();
        }
        else
        {
            triples = triples.OrderBy(p => p[0].rank).ToList();
        }

        foreach (List<CardData> triple in triples)
        {
            if (gameHand == null || HandEvaluator.CompareHand(triple, gameHand))
            {
                return triple;
            }
        }

        return null;
    }

    private List<CardData> GetFiver(List<CardData> gameHand)
    {
        HandType handType = HandEvaluator.EvaluateHand(gameHand);

        if (handType < HandType.Straight)
        {
            return null;
        }

        if(handType == HandType.Straight)
        {
            return GetStraight(gameHand) ?? GetFlush(gameHand) ?? GetFullHouse(gameHand) ?? GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
        }
        else if(handType == HandType.Flush)
        {
            return GetFlush(gameHand) ?? GetFullHouse(gameHand) ?? GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
        }
        else if(handType == HandType.FullHouse)
        {
            return GetFullHouse(gameHand) ?? GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
        }
        else if(handType == HandType.FourOfAKind)
        {
            return GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
        }
        else if(handType == HandType.StraightFlush)
        {
            return GetStraightFlush(gameHand);
        }

        return null;
    }

    public List<CardData> GetStraight(List<CardData> gameHand)
    {
        gameHand = gameHand.SortBySuits();
        gameHand = gameHand.SortByRank();

        var groupedList = playerCards.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });
        groupedList = groupedList.OrderBy(g => g.key);

        List<List<CardData>> rankList = new List<List<CardData>>();
        //converting grouped ienumerable to generic list because crash (???)
        foreach (var group in groupedList)
        {
            List<CardData> list = new List<CardData>(group.list);
            list = list.SortBySuits();
            list = list.SortByRank();
            rankList.Add(list);
        }

        List<CardData> tempHand = new List<CardData>();

        int w = 0;
        
        while(w < rankList.Count - 5)
        {
            if(tempHand.Count == 0) //making sure
            {
                int x = 0;

                while(x < 5)
                {
                    tempHand.Add(rankList[w + x][0]);
                    x++;
                }

                if(tempHand.IsStraight())
                {
                    if(HandEvaluator.CompareStraight(tempHand,gameHand))
                    {
                        return tempHand;
                    }
                    else if(tempHand[4].rank == gameHand[4].rank) //if straight has same head
                    {
                        //try changing head if straight lose
                        int y = 1;
                        int yMax = rankList[w + x - 1].Count;

                        while(y < yMax)
                        {
                            tempHand[4] = rankList[w + x - 1][y];

                            if (HandEvaluator.CompareStraight(tempHand, gameHand))
                            {
                                return tempHand;
                            }

                            y++;
                        }
                    }
                }
            }

            tempHand.Clear();
            w++;
        }

        return null;
    }

    public List<CardData> GetFlush(List<CardData> gameHand)
    {
        var groupedList = playerCards.GroupBy(c => c.suit).Select(group => new { key = group.Key, list = group.ToList() });

        foreach (var group in groupedList)
        {
            if(group.list.Count >= 5)
            {
                if(group.list.Count > 5) //more than 5 of the same suit
                {
                    Debug.Log("more than 5");
                    List<CardData> sortedList = group.list.SortBySuits();

                    List<CardData> tempHand = new List<CardData>();

                    for (int i = 0; i < 5; i++)
                    {
                        tempHand.Add(sortedList[i]);
                    }

                    if(HandEvaluator.CompareFlush(tempHand,gameHand)) //if weakest flush wins against hand
                    {
                        return tempHand;
                    }
                    else if(gameHand.IsFlush() && gameHand[0].suit == group.list[0].suit) //check if game hand is also flush of the same suit
                    {
                        int w = 5;

                        while(w < sortedList.Count)
                        {
                            Debug.Log("w: " + w);
                            tempHand[4] = sortedList[w]; //change flush head
                            if (HandEvaluator.CompareFlush(tempHand, gameHand))
                            {
                                return tempHand;
                            }
                            w++;
                        }
                    }
                }
                else //exactly 5
                {
                    Debug.Log("exactly 5");
                    if(HandEvaluator.CompareFlush(group.list,gameHand))
                    {
                        return group.list;
                    }
                }
            }
        }

        return null;
    }

    public List<CardData> GetFullHouse(List<CardData> gameHand)
    {
        var groupedList = playerCards.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });
        groupedList = groupedList.OrderBy(g => g.key);

        List<List<CardData>> pairs = new List<List<CardData>>();
        List<List<CardData>> triples = new List<List<CardData>>();

        foreach (var group in groupedList)
        {
            List<CardData> sortedList = new List<CardData>(group.list);
            sortedList = sortedList.SortBySuits();

            if(sortedList.Count >= 2)
            {
                var pair = new List<CardData>();
                for (int i = 0; i < 2; i++)
                {
                    pair.Add(sortedList[i]);
                }
                pairs.Add(pair);
            }

            if(sortedList.Count >= 3)
            {
                var triple = new List<CardData>();
                for (int i = 0; i < 3; i++)
                {
                    triple.Add(sortedList[i]);
                }
                triples.Add(triple);
            }
        }

        List<CardData> tempHand = new List<CardData>();

        if(pairs.Count > 0 && triples.Count > 0)
        {
            for (int i = 0; i < triples.Count; i++)
            {
                for (int j = 0; j < pairs.Count; j++)
                {
                    if(triples[i][0].rank != pairs[j][0].rank)
                    {
                        tempHand = new List<CardData>(triples[i].Union(pairs[j]));

                        if (HandEvaluator.CompareMiddle(tempHand, gameHand))
                        {
                            return tempHand;
                        }
                    }

                    tempHand.Clear();
                }
            }
        }

        return null;
    }

    public List<CardData> GetFourKind(List<CardData> gameHand)
    {
        var groupedList = playerCards.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });
        groupedList = groupedList.OrderBy(g => g.key);

        List<CardData> sortedHand = playerCards.SortByRank();

        List<List<CardData>> quads = new List<List<CardData>>();

        foreach (var group in groupedList)
        {
            if(group.list.Count == 4)
            {
                List<CardData> quad = new List<CardData>(group.list);

                int w = 0;

                while(quad.Count < 5)
                {
                    if(!quad.Contains(sortedHand[w]))
                    {
                        quad.Add(sortedHand[w]);
                    }
                    w++;
                }

                if (HandEvaluator.CompareMiddle(quad, gameHand))
                {
                    return quad;
                }
            }
        }

        return null;
    }

    public List<CardData> GetStraightFlush(List<CardData> gameHand)
    {
        List<CardData> sortedHand = playerCards.SortByRank();
        var groupedList = sortedHand.GroupBy(c => c.suit).Select(group => new { key = group.Key, list = group.ToList() }).OrderBy(g => g.key);

        foreach (var group in groupedList)
        {
            if (group.list.Count >= 5)
            {
                List<CardData> tempHand = new List<CardData>();

                int w = 0;

                while (w < group.list.Count - 5)
                {
                    if(tempHand.Count == 0)
                    {
                        tempHand.Add(group.list[w]);
                        for (int i = 1; i < 5; i++)
                        {
                            if(tempHand[tempHand.Count-1].rank != group.list[w+i].rank - 1)
                            {
                                break;
                            }
                            tempHand.Add(group.list[w + i]);
                        }
                    }

                    if(tempHand.IsStraightFlush())
                    {
                        if(HandEvaluator.CompareHand(tempHand,gameHand))
                        {
                            return tempHand;
                        }
                    }

                    tempHand.Clear();
                    w++;
                }
            }
        }


        return null;
    }

    #endregion

    #region First

    public void FirstPlay()
    {
        List<CardData> botHand = GetFirstHands();

        if(botHand == null)
        {
            return;
        }

        botHand = botHand.SortBySuits();
        botHand = botHand.SortByRank();

        Debug.Log(botHand.Count);

        string s = string.Empty;

        foreach (CardData cd in botHand)
        {
            s += cd.rank + " of " + cd.suit + "\t";
        }
        Debug.Log(s);
    }

    public List<CardData> GetFirstHands()
    {
        return GetFirstStraightFlush() ?? GetFirstFourKind() ?? GetFirstFullHouse() ?? GetFirstFlush() ?? GetFirstStraight() ?? GetFirstFlush() 
            ?? GetFirstTriple() ?? GetFirstPair() ?? GetFirstSingle() ?? null;
    }

    public List<CardData> GetFirstSingle()
    {
        Debug.Log("Try single");
        return new List<CardData>() { playerCards.Find(c => c.rank == Card.Rank.Three && c.suit == Card.Suit.Diamonds) };
    }

    public List<CardData> GetFirstPair()
    {
        Debug.Log("Try pair");
        var threesList = playerCards.FindAll(c => c.rank == Card.Rank.Three);
        if(threesList.Count > 2)
        {
            threesList.RemoveRange(2, threesList.Count - 2);
            return threesList;
        }
        else if(threesList.Count < 2)
        {
            return null;
        }

        return threesList;
    }

    public List<CardData> GetFirstTriple()
    {
        Debug.Log("Try triple");
        var threesList = playerCards.FindAll(c => c.rank == Card.Rank.Three);
        Debug.Log("triple three count: " + threesList.Count);
        if (threesList.Count > 3)
        {
            threesList.RemoveRange(3, threesList.Count - 3);
            return threesList;
        }
        else if (threesList.Count < 3)
        {
            return null;
        }

        return threesList;
    }

    public List<CardData> GetFirstStraight()
    {
        Debug.Log("Try straight");
        var sortedHand = playerCards.SortByRank();
        sortedHand = sortedHand.SortBySuits();

        int tempRank = (int)Card.Rank.Three;

        var tempHand = new List<CardData>();
        tempHand.Add(sortedHand[0]);

        while (tempHand.Count < 5)
        {
            tempRank++;

            if (!sortedHand.Exists(c => c.rank == (Card.Rank)tempRank))
            {
                return null;
            }

            tempHand.Add(sortedHand.Find(c => c.rank == (Card.Rank)tempRank));
        }

        return tempHand;
    }

    public List<CardData> GetFirstFlush()
    {
        Debug.Log("Try flush");
        var diamondsList = playerCards.FindAll(c => c.suit == Card.Suit.Diamonds);
        diamondsList = diamondsList.SortByRank();

        if (diamondsList.Count == 5)
        {
            return diamondsList;
        }
        else if (diamondsList.Count > 5)
        {
            diamondsList.RemoveRange(5, diamondsList.Count - 5);
            return diamondsList;
        }
        else
        {
            return null;
        }
    }

    public List<CardData> GetFirstFullHouse()
    {
        Debug.Log("Try full house");
        var tempHand = playerCards.FindAll(c => c.rank == Card.Rank.Three);

        if (tempHand.Count < 2 || tempHand.Count == 4) // if cant make full house or is four of a kind
        {
            return null;
        }

        List<CardData> removedThreesList = new List<CardData>(playerCards);
        //removedThreesList.Remove(tempHand.ToArray());
        removedThreesList.RemoveAll(c => c.rank == Card.Rank.Three);
        var groupedList = removedThreesList.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });
        groupedList = groupedList.OrderBy(g => g.key);

        var convertedList = groupedList as List<List<CardData>>;

        if (groupedList.Count() == 0)
        {
            return null;
        }

        if (tempHand.Count == 3)
        {
            foreach (var group in groupedList)
            {
                if(group.list.Count < 2)
                {
                    continue;
                }

                for (int i = 0; i < 2; i++)
                {
                    tempHand.Add(group.list[i]);
                }
                return tempHand;
            }
        }
        else if (tempHand.Count == 2)
        {
            foreach (var group in groupedList)
            {
                if (group.list.Count < 3)
                {
                    continue;
                }

                for (int i = 0; i < 3; i++)
                {
                    tempHand.Add(group.list[i]);
                }
                return tempHand;
            }
        }

        return null;
    }

    public List<CardData> GetFirstFourKind()
    {
        Debug.Log("Try four of a kind");
        var threesList = playerCards.FindAll(c => c.rank == Card.Rank.Three);

        List<CardData> removedThreesList = new List<CardData>(playerCards);
        removedThreesList.RemoveAll(c => c.rank == Card.Rank.Three);

        if (threesList.Count == 4)
        {
            var sortedList = removedThreesList.SortBySuits();
            sortedList = sortedList.SortByRank();
            threesList.Add(sortedList[0]);
            return threesList;
        }

        return null;
    }

    public List<CardData> GetFirstStraightFlush()
    {
        Debug.Log("Try straight flush");
        var tempHand = playerCards.FindAll(c => c.suit == Card.Suit.Diamonds);

        tempHand = tempHand.SortByRank();
        Debug.Log(tempHand[0].rank + " of " + tempHand[0].suit);

        if (tempHand.Count < 5)
        {
            return null;
        }

        if (tempHand.Count > 5)
        {
            tempHand.RemoveRange(5, tempHand.Count - 5);
        }

        for (int i = 1; i < 5; i++)
        {
            if (tempHand[i].rank != (tempHand[i - 1].rank + 1))
            {
                return null;
            }
        }

        return tempHand;
    }

    #endregion

    public void PrintPossibleHands()
    {
        Combinations<CardData> cardCombinations = new Combinations<CardData>(playerCards, 5);

        int length = (int)cardCombinations.Count;
        Debug.Log("Total combinations: "+length);

        List<List<CardData>> validHands = new List<List<CardData>>();

        foreach (List<CardData> hand in cardCombinations)
        {
            if (hand.IsFlush() || hand.IsStraight() || hand.IsFourKind() || hand.IsFullHouse() || hand.IsStraightFlush())
            {
                validHands.Add(hand);
            }
        }

        Debug.Log(validHands.Count);

        foreach (List<CardData> hand in validHands)
        {
            string s = "";
            if (hand.IsStraightFlush())
            {
                s += "Straight Flush";
            }
            else if (hand.IsFourKind())
            {
                s += "Four of a kind";
            }
            else if (hand.IsFullHouse())
            {
                s += "Full House";
            }
            else if (hand.IsFlush())
            {
                s += "Flush";
            }
            else if (hand.IsStraight())
            {
                s += "Straight";
            }

            s += ": ";

            foreach (CardData card in hand)
            {
                s += StringEnum.GetStringValue(card.rank) + " " + Enum.GetName(typeof(Card.Suit), card.suit) + ", ";
            }
            Debug.Log(s);
        }
    }

}
