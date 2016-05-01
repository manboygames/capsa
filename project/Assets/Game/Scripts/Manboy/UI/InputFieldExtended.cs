using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputFieldExtended : InputField {

    [Serializable]
    public class KeyboardDoneEvent : UnityEvent { }

    [SerializeField]
    private KeyboardDoneEvent m_keyboardDone = new KeyboardDoneEvent();

    public KeyboardDoneEvent onKeyboardDone
    {
        get { return m_keyboardDone; }
        set { m_keyboardDone = value; }
    }

    void Update()
    {
        if (m_Keyboard != null && m_Keyboard.done && !m_Keyboard.wasCanceled)
        {
            m_keyboardDone.Invoke();
        }
    }
}
