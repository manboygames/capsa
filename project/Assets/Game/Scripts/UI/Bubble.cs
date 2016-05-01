using UnityEngine;
using DG.Tweening;
using AdvancedInspector;
using System;

public class Bubble : MonoBehaviour {
    
    public enum Direction
    {
        NorthEast,
        NorthWest,
        SouthWest,
        SouthEast
    }

    public Transform pointer;
    public RectTransform bubble;

    private CanvasGroup canvasGroup;
    private Sequence bubbleSequence;

    [Inspect, ReadOnly]
    private Direction currentDirection;

    [Serializable]
    public class BubbleDictionary : UDictionary<Direction, BubbleOrientation> { }

    public BubbleDictionary bubbleOrientations = new BubbleDictionary();

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        transform.localScale = Vector3.zero;
    }

    void Start()
    {
        bubbleSequence = DOTween.Sequence();
    }

    public void SetDirection(Direction direction)
    {
        bubble.pivot = bubbleOrientations[direction].bubblePivot;
        Vector2 offset = bubbleOrientations[direction].bubbleOffset;
        bubble.localPosition = new Vector2(offset.x, offset.y);

        Vector2 pRot = bubbleOrientations[direction].pointerRotation;
        //pointer.localEulerAngles = new Vector3(pRot.x,pRot.y,pointer.localEulerAngles.z);
        pointer.localEulerAngles = new Vector3(pRot.x, pRot.y, -30f);

        currentDirection = direction;
    }

    public void ShowBubble(float showDuration = 1f)
    {

        if(bubbleSequence.IsPlaying())
        {
            bubbleSequence.Complete();
        }

        //AudioManager.Instance.PlaySFX("Bubble");

        bubbleSequence = DOTween.Sequence();
        bubbleSequence.Append(transform.DOScale(new Vector3(1, 1, 1), 0.2f));
        bubbleSequence.AppendInterval(showDuration);
        bubbleSequence.Append(canvasGroup.DOFade(0, 0.5f).OnComplete(RefreshBubble));
        bubbleSequence.Play();
    }

    public void StopTween()
    {
        if(bubbleSequence == null || !bubbleSequence.IsActive())
        {
            return;
        }
        if(bubbleSequence.IsPlaying())
        {
            bubbleSequence.Kill(true);
        }
    }

    private void RefreshBubble()
    {
        transform.localScale = Vector3.zero;
        canvasGroup.Show(true);
    }
}

[Serializable]
public struct BubbleOrientation
{
    public Vector2 pointerRotation;
    public Vector2 bubblePivot;
    public Vector2 bubbleOffset;
}
