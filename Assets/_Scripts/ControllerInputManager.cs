using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerInputManager : MonoBehaviour
{
    public GameObject leftHand;
    [SerializeField]
    private GameObject _rightHand;    
    [SerializeField]
    private XRDirectInteractor _rightDirectInteractor;
    public float diff_threshold=0.05f; //threshold by which the stop teleportation occurs.
    [SerializeField]
    InputActionProperty m_gripPressed;
    [SerializeField]
    InputActionProperty m_secondaryButton;
    [SerializeField]
    InputActionProperty m_menuButton;
    [SerializeField]
    InputActionProperty m_LeftActivate;
    [SerializeField]
    InputActionProperty m_RightSelect;
    [SerializeField]
    InputActionProperty m_LeftJoystick;
    public static Action onGripPressed;
    public static Action onSecondaryPress;

    public static Action onMenuPressed;
    private XRRayInteractor _leftRayInteractor;
    private XRRayInteractor _rightRayInteractor;
    private TeleportationProvider _teleportationProvider;
    private float _last_thumbstick_y; //last saved thumbstick y value
    // Start is called before the first frame update

    void Awake(){
        m_gripPressed.action.performed += HandleCanvasText;
        m_secondaryButton.action.performed += HandleSpeechRec;
        m_menuButton.action.performed += HandleMenu;
        m_LeftJoystick.action.performed += HandleTeleportation;
        m_RightSelect.action.performed += HandleInteractors;
        _leftRayInteractor = leftHand.GetComponent<XRRayInteractor>();
        _teleportationProvider = GetComponent<TeleportationProvider>();
        _rightRayInteractor = _rightHand.GetComponent<XRRayInteractor>();
    }

    private void HandleInteractors(InputAction.CallbackContext context)
    {
        if(_rightRayInteractor.enabled)
        {
            _rightRayInteractor.enabled = false;
            _rightDirectInteractor.enabled = true;
        }
        else
        {
            _rightDirectInteractor.enabled = false;
            _rightRayInteractor.enabled = true;
        }
    }

    //Teleportation happens here
    //Stops the ray interactor
    private void StopTeleportation(InputAction.CallbackContext context)
    {
        
        if(_leftRayInteractor.enabled)
        {
            if (_leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit raycastHit)) // if it hits something
            {
                if(raycastHit.transform.gameObject.TryGetComponent<TeleportationArea>(out TeleportationArea tp)) //If this something has a teleportation area or anchor
                {
                    TeleportRequest teleportRequest = new TeleportRequest()
                    {
                        destinationPosition = raycastHit.point,
                    };
                    _teleportationProvider.QueueTeleportRequest(teleportRequest);
                }

            }
            _leftRayInteractor.enabled = false;
        }
    }

    private void HandleMenu(InputAction.CallbackContext context)
    {
        onMenuPressed?.Invoke();
    }

    //Teleportation with thumbstick
    private void HandleTeleportation(InputAction.CallbackContext context)
    {
        float t = m_LeftJoystick.action.ReadValue<Vector2>().y;
        if(_last_thumbstick_y > t + diff_threshold)
        {
            StopTeleportation(context);
        }
        else if(t>0.1)
        {
            _last_thumbstick_y = t;
            t = m_LeftJoystick.action.ReadValue<Vector2>().y;
            _leftRayInteractor.enabled = true;
            float endPointDistance = Mathf.Lerp(-12, 20, t);
            float endPointheight = Mathf.Lerp(-60, -15, t);
            _leftRayInteractor.endPointDistance = endPointDistance;
            _leftRayInteractor.endPointHeight = endPointheight;
        }
       
    }

    private void HandleSpeechRec(InputAction.CallbackContext context)
    {
        onSecondaryPress?.Invoke();
    }

    private void HandleCanvasText(InputAction.CallbackContext context)
    {
        onGripPressed?.Invoke();
    }

    void OnDisable(){
        m_gripPressed.action.performed -= HandleCanvasText;
    }
}
