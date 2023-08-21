using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TouchButton : XRBaseInteractable
{
    [SerializeField]
    private Transform _buttonTransform;
    public static Action OnLeftPress;
    public static Action OnRightPress;
    public static Action OnValidatePress;
    public static Action OnTestPress;
    private Vector3 _initialPosition;
    private void Start() {
        _initialPosition = _buttonTransform.localPosition;
    }

    public enum Buttons {
        Left,
        Right,
        Validate,
        Test
    };

    [SerializeField]
    private Buttons _BtnType;

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        _buttonTransform.localPosition = new Vector3(_initialPosition.x, _initialPosition.y-0.5f, _initialPosition.z);
        switch (_BtnType)
        {
            case Buttons.Left:
                OnLeftPress?.Invoke();
                break;
            case Buttons.Right:
                OnRightPress?.Invoke();
                break;
            case Buttons.Validate:
                OnValidatePress?.Invoke();
                break;
            case Buttons.Test:
                OnTestPress?.Invoke();
                break;
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        _buttonTransform.localPosition = _initialPosition;
    }
}
