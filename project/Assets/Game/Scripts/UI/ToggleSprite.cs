using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Toggle))]
public class ToggleSprite : MonoBehaviour {

    private Toggle toggle;

    public Image targetImage;
    public Sprite offSprite;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        SwapSprite(toggle.isOn);
        toggle.onValueChanged.AddListener(SwapSprite);
    }

    public void SwapSprite(bool isOn)
    {
        if(isOn)
        {
            targetImage.overrideSprite = null;
        }
        else
        {
            targetImage.overrideSprite = offSprite;
        }
    }
}
