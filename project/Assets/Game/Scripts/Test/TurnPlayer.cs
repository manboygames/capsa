using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.Events;

public class TurnPlayer : MonoBehaviour {

    private SpriteRenderer sprite;

    [System.Serializable]
    public class OnPlayerDestroyedEvent : UnityEvent<TurnPlayer> { }

    [HideInInspector]
    public OnPlayerDestroyedEvent onDestroy = new OnPlayerDestroyedEvent();

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Activate(bool active)
    {
        float a = active ? 1 : 0.5f;
        sprite.DOFade(a, 0);
    }

    void OnDestroy()
    {
        onDestroy.Invoke(this);
    }

}
