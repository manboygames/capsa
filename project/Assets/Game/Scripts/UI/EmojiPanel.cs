using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EmojiPanel : MonoBehaviour {

    public EmojiController emojiController;
    public ScrollSnapRect scroller;

    private CanvasGroup scrollerGroup;

    private bool shown;

    private int tempPage;

    void Awake()
    {
        scrollerGroup = scroller.GetComponent<CanvasGroup>();
        emojiController.onEmojiClicked.AddListener(OnEmojiClicked);
        ShowPanel(false, false);
    }

    private void OnEmojiClicked(Sprite sprite)
    {
        //ShowPanel(false, true);
        TogglePanel();
    }

    public void TogglePanel()
    {
        shown = !shown;
        ShowPanel(shown);
    }

    public void Deselect()
    {
        Debug.Log("DE");
    }

    private void ShowPanel(bool show, bool tween = true)
    {
        scrollerGroup.Activate(!show);

        if (!show)
        {
            tempPage = scroller.CurrentPage;
        }

        float scale = show ? 1 : 0;
        int sfxId = show ? 0 : 1;

        if(tween)
        {
            //AudioManager.Instance.PlaySFX("Bubble", sfxId);
            Tweener tweener = transform.DOScale(scale, 0.15f);
            tweener.OnComplete(RefreshScroller);
        }
        else
        {
            transform.localScale = new Vector3(scale, scale, scale);
            scrollerGroup.Activate(true);
        }
    }

    private void RefreshScroller()
    {
        scroller.SetPage(tempPage);
        scrollerGroup.Activate(true);
    }

}
