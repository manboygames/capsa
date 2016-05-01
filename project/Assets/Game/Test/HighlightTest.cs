using UnityEngine;
using System.Collections;
using HighlightingSystem;

public class HighlightTest : MonoBehaviour {

    Highlighter highlighter;

    void Awake()
    {
        highlighter = GetComponent<Highlighter>();
    }

    void Start()
    {
        highlighter.FlashingOn(Color.white, Color.yellow);
    }

}
