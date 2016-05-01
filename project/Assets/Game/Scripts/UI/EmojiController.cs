using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class EmojiController : MonoBehaviour {

    private static EmojiController instance;

    public static EmojiController Instance
    {
        get
        {
            return instance;
        }

        set
        {
            instance = value;
        }
    }

    public GameObject emojiContainer;

    private List<Sprite> emojiSprites = new List<Sprite>();

    [System.Serializable]
    public class OnEmojiButtonClicked : UnityEvent<Sprite> { }

    [HideInInspector]
    public OnEmojiButtonClicked onEmojiClicked = new OnEmojiButtonClicked();

    void Awake()
    {
        instance = this;

        Image[] images = emojiContainer.GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            emojiSprites.Add(image.sprite);
        }
    }

    public void EmojiButtonClick(Image image)
    {
        onEmojiClicked.Invoke(emojiSprites.Find(s => s == image.sprite));
    }

    public Sprite GetSprite(string spriteName)
    {
        return emojiSprites.Find(s => s.name == spriteName);
    }

}
