using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EmojiBubble : Bubble {

    public Image emoji;

    public void SetEmoji(Sprite sprite)
    {
        emoji.sprite = sprite;
    }
}
