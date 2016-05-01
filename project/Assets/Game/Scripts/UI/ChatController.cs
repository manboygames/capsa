using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class ChatController : MonoBehaviour {

    public InputFieldExtended inputField;

    private static ChatController instance;

    public static ChatController Instance
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

    [System.Serializable]
    public class OnChatSubmit : UnityEvent<string> { }

    [HideInInspector]
    public OnChatSubmit onChatSubmit = new OnChatSubmit();

    void Awake()
    {
        instance = this;

        
    }

    void OnEnable()
    {
        inputField.onEndEdit.AddListener(OnEndEdit);

        if (Application.isMobilePlatform)
        {
            inputField.onKeyboardDone.AddListener(OnKeyboardDone);
        }
    }

    void OnDisable()
    {
        inputField.onEndEdit.RemoveListener(OnEndEdit);

        if (Application.isMobilePlatform)
        {
            inputField.onKeyboardDone.RemoveListener(OnKeyboardDone);
        }
    }

    private void OnEndEdit(string value)
    {
        if (!Application.isMobilePlatform &&Input.GetButtonDown("Submit"))
        {
            onChatSubmit.Invoke(inputField.text);
        }

        inputField.text = string.Empty;
        inputField.interactable = false;
    }

    private void OnKeyboardDone()
    {
        onChatSubmit.Invoke(inputField.text);
    }

    public void ActivateInput()
    {
        inputField.interactable = !inputField.interactable;

        if(inputField.interactable)
        {
            inputField.ActivateInputField();
            inputField.Select();
        }
    }
}
