using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class ToggleExtended : Toggle
{
    public enum VisualState
    {
        Normal,
        Highlighted,
        Pressed,
        Disabled
    }

    public UnityEvent InteractableChanged;

    [Serializable]
    public class StateChangeEvent : UnityEvent<VisualState, bool> { }

    public StateChangeEvent onStateChange = new StateChangeEvent();

    protected override void OnDisable()
    {
        base.OnDisable();

        if (this.onStateChange != null)
        {
            this.onStateChange.Invoke(VisualState.Disabled, true);
        }
    }

    protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        this.onStateChange.Invoke((VisualState)state, instant);
    }

    [Serializable]
    public class OnValueChangeExtended : UnityEvent<ToggleExtended, bool> { }

    public OnValueChangeExtended onValueChangedExtended = new OnValueChangeExtended();

    protected override void Awake()
    {
        base.Awake();
        onValueChanged.AddListener(OnValueChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onValueChanged.RemoveListener(OnValueChanged);
    }

    void OnValueChanged(bool value)
    {
        onValueChangedExtended.Invoke(this, value);
    }


}