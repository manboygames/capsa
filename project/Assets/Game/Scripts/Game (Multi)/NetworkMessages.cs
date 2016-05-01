using UnityEngine;
using UnityEngine.Networking;

//client to server
class ClientPlayerReadyMsg:MessageBase
{
    public static short msgID = 1001;
}

class ClientPlayHandMsg:MessageBase
{
    public static short msgID = 1002;
    public CardData[] cardDatas;
    public HandType handType;
    public int remainingCards;
    public bool playingSet;
}

class ClientPassTurnMsg:MessageBase
{
    public static short msgID = 1003;
}

class ClientPlayEmojiMsg:MessageBase
{
    public static short msgID = 1004;
    public string spriteName;
}

class ClientSubmitChatMsg:MessageBase
{
    public static short msgID = 1005;
    public string chatMsg;
}

class ClientRematchMessage:MessageBase
{
    public static short msgID = 1006;
    public bool rematch;
}

class ClientDestroyedMessage:MessageBase
{
    public static short msgID = 1007;
    public int playerID;
}

//server to client
class ServerAddCardsMsg : MessageBase
{
    public static short msgID = 2001;
    public CardData[] cardDatas;
}

class ServerAssignTurnMsg:MessageBase
{
    public static short msgID = 2002;
    public bool isTurn;
    public bool firstTurn;
}

class ServerDecideWinnerMsg : MessageBase
{
    public static short msgID = 2003;
}
