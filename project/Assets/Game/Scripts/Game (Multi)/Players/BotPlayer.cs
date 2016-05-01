using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class BotPlayer : GamePlayer
{

    public enum HandStrength
    {
        Min,
        Max,
        Random
    }

    public int botID;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer)
        {
            CmdUpdatePlayerID();
            CmdChangeName("Bot " + (botID + 1).ToString());
            CmdChangeCharacterID(Random.Range(0, CharacterManager.Instance.characters.Count));
        }
    }

    [Command]
    private void CmdUpdatePlayerID()
    {
        playerID = botID + LobbyManager.Instance.numPlayers;
    }

    public override void AssignTurn(bool isTurn, bool firstTurn)
    {
        base.AssignTurn(isTurn, firstTurn);

        if (isTurn)
        {
            StartCoroutine(BotThink());
        }
    }

    private IEnumerator BotThink()
    {
        //TODO: fix this
        yield return new WaitForSeconds(1.5f);

        HandType handType;

        switch (gameManager.gameState)
        {
            case GameState.FirstRound:
                handType = PlayHand(true, true);
                break;
            case GameState.NewRound:
                handType = PlayHand(true, false);
                break;
            case GameState.GameRound:
                handType = PlayHand(false, false);
                break;
            default:
                handType = HandType.Invalid;
                break;
        }

        if (handType == HandType.Invalid)
        {
            PassTurn();
        }
    }

    protected override HandType PlayHand(bool newRound, bool requireLowestCard)
    {
        HandType handType = HandType.Invalid;

        List<CardData> tempHand = new List<CardData>();

        if (requireLowestCard)
        {
            Debug.Log(playerName + " is playing in first round");

            tempHand = BotFirstPlays();

            if (tempHand != null)
            {
                handType = HandEvaluator.EvaluateHand(tempHand);

                var msg = new ClientPlayHandMsg();
                msg.cardDatas = tempHand.ToArray();
                msg.handType = handType;
                msg.remainingCards = hand.RemainingCards;
                msg.playingSet = tempHand.Count == 5; //if hand is a set
                client.Send(ClientPlayHandMsg.msgID, msg);

                return HandEvaluator.EvaluateHand(tempHand);
            }
        }

        if (newRound)
        {
            Debug.Log(playerName + " is first player in new round");

            tempHand = BotPlays();

            if (tempHand != null)
            {
                hand.RemainingCards -= tempHand.Count;

                handType = HandEvaluator.EvaluateHand(tempHand);

                var msg = new ClientPlayHandMsg();
                msg.cardDatas = tempHand.ToArray();
                msg.handType = handType;
                msg.remainingCards = hand.RemainingCards;
                msg.playingSet = tempHand.Count == 5; //if hand is a set
                client.Send(ClientPlayHandMsg.msgID, msg);

                return handType;
            }
        }

        List<CardData> gameHand = gameManager.currentPlayedHand.ToDynList();

        int gameHandCount = gameHand.Count;

        if (hand.cardDatas.Count < gameHandCount)
        {
            //return invalid if amount of cards less than played hand in game
            return HandType.Invalid;
        }

        tempHand = BotCounterPlays(gameHandCount);

        if (tempHand == null) //if bot has valid hand to play
        {
            //bot pass
            return HandType.Invalid;
        }
        else
        {
            //send hand to server
            hand.RemainingCards -= tempHand.Count;

            handType = HandEvaluator.EvaluateHand(tempHand);

            var msg = new ClientPlayHandMsg();
            msg.cardDatas = tempHand.ToArray();
            msg.handType = handType;
            msg.remainingCards = hand.RemainingCards;
            msg.playingSet = tempHand.Count == 5; //if hand is a set
            client.Send(ClientPlayHandMsg.msgID, msg);

            return handType;
        }
    }

    private List<CardData> BotFirstPlays()
    {
        return GetFirstStraightFlush() ?? GetFirstFourKind() ?? GetFirstFullHouse() ?? GetFirstFlush() ?? GetFirstStraight() ?? GetFirstFlush()
            ?? GetFirstTriple() ?? GetFirstPair() ?? GetFirstSingle() ?? null;
    }

    private List<CardData> GetFirstSingle()
    {
        return new List<CardData>() { hand.cardDatas.ToList().Find(c => c.rank == Card.Rank.Three && c.suit == Card.Suit.Diamonds) };
    }

    private List<CardData> GetFirstPair()
    {
        var threesList = hand.cardDatas.ToList().FindAll(c => c.rank == Card.Rank.Three);
        if (threesList.Count > 2)
        {
            threesList.RemoveRange(2, threesList.Count - 2);
            return threesList;
        }
        else if (threesList.Count < 2)
        {
            return null;
        }

        return threesList;
    }

    private List<CardData> GetFirstTriple()
    {
        var threesList = hand.cardDatas.ToList().FindAll(c => c.rank == Card.Rank.Three);

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

    private List<CardData> GetFirstStraight()
    {
        var sortedHand = hand.cardDatas.ToList().SortByRank();
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

    private List<CardData> GetFirstFlush()
    {
        var diamondsList = hand.cardDatas.ToList().FindAll(c => c.suit == Card.Suit.Diamonds);
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

    private List<CardData> GetFirstFullHouse()
    {
        var tempHand = hand.cardDatas.ToList().FindAll(c => c.rank == Card.Rank.Three);

        if (tempHand.Count < 2 || tempHand.Count == 4) // if cant make full house or is four of a kind
        {
            return null;
        }

        List<CardData> removedThreesList = new List<CardData>(hand.cardDatas.ToList());
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
                if (group.list.Count < 2)
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

    private List<CardData> GetFirstFourKind()
    {
        var threesList = hand.cardDatas.ToList().FindAll(c => c.rank == Card.Rank.Three);

        List<CardData> removedThreesList = new List<CardData>(hand.cardDatas.ToList());
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

    private List<CardData> GetFirstStraightFlush()
    {
        var tempHand = hand.cardDatas.ToList().FindAll(c => c.suit == Card.Suit.Diamonds);

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


    private List<CardData> BotPlays()
    {
        bool playStrong = gameManager.GetNextPlayer().Hand.cardDatas.Count <= 5;

        if(playStrong)
        {
            return GetStraightFlush(null) ?? GetFourKind(null) ?? GetFullHouse(null) ?? GetFlush(null) ?? GetStraight(null)
                ?? GetTriple(null,true) ?? GetPair(null,true) ?? GetSingle(null,true) ;
        }
        
        return GetStraight(null) ?? GetFlush(null) ?? GetFullHouse(null) ?? GetFourKind(null) ?? GetStraightFlush(null)
            ?? GetPair(null) ?? GetTriple(null) ?? GetSingle(null);
    }

    private List<CardData> BotCounterPlays(int gameHandCount)
    {
        List<CardData> gameHand = gameManager.currentPlayedHand.ToList();

        bool playStrong = gameManager.GetNextPlayer().Hand.cardDatas.Count <= 5;

        switch (gameHandCount)
        {
            case 1:
                return GetSingle(gameHand, playStrong);

            case 2:
                return GetPair(gameHand, playStrong);

            case 3:
                return GetTriple(gameHand, playStrong);

            case 5:
                return GetFiver(gameHand, playStrong);
        }

        return null;
    }

    private List<CardData> GetSingle(List<CardData> gameHand, bool playStrong = false)
    {
        List<CardData> sortedHand = hand.cardDatas.ToDynList().SortBySuits(playStrong);
        sortedHand = sortedHand.SortByRank(playStrong);

        int count = sortedHand.Count;

        if (playStrong)
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

    private List<CardData> GetPair(List<CardData> gameHand, bool playStrong = false)
    {
        var groupedList = hand.cardDatas.ToList().GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });

        if (playStrong)
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

        if (playStrong)
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

    private List<CardData> GetTriple(List<CardData> gameHand, bool playStrong = false)
    {
        var groupedList = hand.cardDatas.ToList().GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });

        if (playStrong)
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

        if (playStrong)
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


    private List<CardData> GetFiver(List<CardData> gameHand, bool playStrong = false)
    {
        HandType handType = HandEvaluator.EvaluateHand(gameHand);

        if (handType < HandType.Straight)
        {
            return null;
        }

        //List<Action> actions = new List<Action>();

        if (playStrong)
        {
            return GetStraightFlush(gameHand) ?? GetFourKind(gameHand) ?? GetFullHouse(gameHand) ?? GetFlush(gameHand) ?? GetStraight(gameHand);
        }
        else
        {
            if (handType == HandType.Straight)
            {
                return GetStraight(gameHand) ?? GetFlush(gameHand) ?? GetFullHouse(gameHand) ?? GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
            }
            else if (handType == HandType.Flush)
            {
                return GetFlush(gameHand) ?? GetFullHouse(gameHand) ?? GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
            }
            else if (handType == HandType.FullHouse)
            {
                return GetFullHouse(gameHand) ?? GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
            }
            else if (handType == HandType.FourOfAKind)
            {
                return GetFourKind(gameHand) ?? GetStraightFlush(gameHand);
            }
            else if (handType == HandType.StraightFlush)
            {
                return GetStraightFlush(gameHand);
            }
        }

        return null;
    }

    public List<CardData> GetStraight(List<CardData> gameHand)
    {
        //TODO: Strongest
        if (gameHand != null)
        {
            gameHand = gameHand.SortBySuits();
            gameHand = gameHand.SortByRank();
        }

        var groupedList = hand.cardDatas.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });
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

        while (w < rankList.Count - 5) //while count of ranks is enough to make a straight (5)
        {
            if (tempHand.Count == 0) //making sure
            {
                int x = 0;

                int invalidIndex = 0;

                while (x < 5)
                {
                    if(x > 0)
                    {
                        if(rankList[w+x][0].rank != rankList[w+x-1][0].rank + 1)
                        {
                            //break out of loop and save index (to w) so it will start from there
                            invalidIndex = w + x;
                            break;
                        }
                    }
                    tempHand.Add(rankList[w + x][0]);
                    x++;
                }

                if(invalidIndex > 0) //not a straight, go to next loop and change current index to the last invalid one
                {
                    w = invalidIndex;
                    continue;
                }

                if (tempHand.IsStraight())
                {
                    if (gameHand == null || HandEvaluator.CompareStraight(tempHand, gameHand))
                    {
                        return tempHand;
                    }
                    else if (tempHand[4].rank == gameHand[4].rank) //if straight has same head
                    {
                        //try changing head if straight lose
                        int y = 1;
                        int yMax = rankList[w + x - 1].Count;

                        while (y < yMax)
                        {
                            tempHand[4] = rankList[w + x - 1][y];

                            if (gameHand == null || HandEvaluator.CompareStraight(tempHand, gameHand))
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
        var groupedList = hand.cardDatas.GroupBy(c => c.suit).Select(group => new { key = group.Key, list = group.ToList() });
        groupedList = groupedList.OrderBy(g => g.key);


        foreach (var group in groupedList)
        {
            if (group.list.Count >= 5)
            {
                if (group.list.Count > 5) //more than 5 of the same suit
                {
                    List<CardData> sortedList = group.list.SortBySuits();

                    List<CardData> tempHand = new List<CardData>();

                    for (int i = 0; i < 5; i++)
                    {
                        tempHand.Add(sortedList[i]);
                    }

                    if (gameHand == null || HandEvaluator.CompareFlush(tempHand, gameHand)) //if weakest flush wins against hand
                    {
                        return tempHand;
                    }
                    else if (gameHand.IsFlush() && gameHand[0].suit == group.list[0].suit) //check if game hand is also flush of the same suit
                    {
                        int w = 5;

                        while (w < sortedList.Count)
                        {
                            Debug.Log("w: " + w);
                            tempHand[4] = sortedList[w]; //change flush head
                            if (gameHand == null || HandEvaluator.CompareFlush(tempHand, gameHand))
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
                    if (gameHand == null || HandEvaluator.CompareFlush(group.list, gameHand))
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
        var groupedList = hand.cardDatas.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });
        groupedList = groupedList.OrderBy(g => g.key);

        List<List<CardData>> pairs = new List<List<CardData>>();
        List<List<CardData>> triples = new List<List<CardData>>();

        foreach (var group in groupedList)
        {
            List<CardData> sortedList = new List<CardData>(group.list);
            sortedList = sortedList.SortBySuits();

            if (sortedList.Count >= 2)
            {
                List<CardData> pair = new List<CardData>();
                for (int i = 0; i < 2; i++)
                {
                    pair.Add(sortedList[i]);
                }
                pairs.Add(pair);
            }

            if (sortedList.Count >= 3)
            {
                List<CardData> triple = new List<CardData>();
                for (int i = 0; i < 3; i++)
                {
                    triple.Add(sortedList[i]);
                }
                triples.Add(triple);
            }
        }

        //pairs = new List<List<CardData>>(pairs.OrderBy(p => p[0].rank));

        List<CardData> tempHand = new List<CardData>();

        if (pairs.Count > 0 && triples.Count > 0)
        {
            for (int i = 0; i < triples.Count; i++)
            {
                for (int j = 0; j < pairs.Count; j++)
                {
                    if (triples[i][0].rank != pairs[j][0].rank)
                    {
                        tempHand = new List<CardData>(triples[i].Union(pairs[j]));

                        if (gameHand == null || HandEvaluator.CompareMiddle(tempHand, gameHand))
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
        var groupedList = hand.cardDatas.GroupBy(c => c.rank).Select(group => new { key = group.Key, list = group.ToList() });
        groupedList = groupedList.OrderByDescending(g => g.key);

        List<CardData> sortedHand = hand.cardDatas.ToList().SortByRank();

        List<List<CardData>> quads = new List<List<CardData>>();

        foreach (var group in groupedList)
        {
            if (group.list.Count == 4)
            {
                List<CardData> quad = new List<CardData>(group.list);

                int w = 0;

                while (quad.Count < 5)
                {
                    if (!quad.Contains(sortedHand[w]))
                    {
                        quad.Add(sortedHand[w]);
                    }
                    w++;
                }

                if (gameHand == null || HandEvaluator.CompareMiddle(quad, gameHand))
                {
                    return quad;
                }
            }
        }

        return null;
    }

    public List<CardData> GetStraightFlush(List<CardData> gameHand)
    {
        //TODO: Strongest
        List<CardData> sortedHand = hand.cardDatas.ToList().SortByRank();
        var groupedList = sortedHand.GroupBy(c => c.suit).Select(group => new { key = group.Key, list = group.ToList() }).OrderBy(g => g.key);

        foreach (var group in groupedList)
        {
            if (group.list.Count >= 5)
            {
                List<CardData> tempHand = new List<CardData>();

                int w = 0;

                while (w < group.list.Count - 5)
                {
                    int invalidIndex = 0;

                    if (tempHand.Count == 0)
                    {
                        tempHand.Add(group.list[w]);
                        for (int i = 1; i < 5; i++)
                        {
                            if (tempHand[tempHand.Count - 1].rank != group.list[w + i].rank - 1)
                            {
                                invalidIndex = w + i;
                                break;
                            }
                            tempHand.Add(group.list[w + i]);
                        }
                    }

                    if(invalidIndex > 0)
                    {
                        w = invalidIndex;
                        continue;
                    }

                    if (tempHand.IsStraightFlush())
                    {
                        if (gameHand == null || HandEvaluator.CompareHand(tempHand, gameHand))
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

}
