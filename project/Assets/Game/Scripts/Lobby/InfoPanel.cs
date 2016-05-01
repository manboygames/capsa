using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class InfoPanel : MonoBehaviour {

    public Text textInfo;
    public Button button;
    private Text textButton;

    [HideInInspector]
    public CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        textButton = button.GetComponentInChildren<Text>();
    }

    public void SetInfoText(string msg)
    {
        textInfo.text = msg;
    }

    public void SetButtonListener(UnityAction callback, string buttonLabel)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
        textButton.text = buttonLabel;
    }

    public void Show(bool shown)
    {
        canvasGroup.alpha = shown ? 1 : 0;
        canvasGroup.interactable = shown;
        canvasGroup.blocksRaycasts = shown;
    }

}
