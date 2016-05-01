using UnityEngine;
using System.Collections;

public static class CanvasGroupExtensions
{
    public static void Activate(this CanvasGroup canvasGroup, bool activate)
    {
        canvasGroup.alpha = activate ? 1 : 0;
        canvasGroup.interactable = activate;
        canvasGroup.blocksRaycasts = activate;
    }

    public static void Show(this CanvasGroup canvasGroup, bool show)
    {
        canvasGroup.alpha = show ? 1 : 0;
    }
}
