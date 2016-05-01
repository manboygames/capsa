using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChatBubble : Bubble {

    public Text textChat;

    public void SetText(string chatMsg)
    {
        textChat.text = chatMsg;
    }

}
