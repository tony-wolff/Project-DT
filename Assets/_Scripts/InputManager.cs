using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [Header("Input Settings")]
    public TMP_InputField  _InputField;
    //Events triggered when user enters prompt, invoke methods from Logic
    public static Action<string> onUserRequest; //Ready-made delegates without needing to separately declare a delegate type
    // Start is called before the first frame update
    public RawImage image_mic;
    void Start()
    {
        _InputField.Select();//Focus on input field
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)) //When users tap "Enter", sends the prompts to events
        {
            Debug.Log(_InputField.text);
            if(_InputField.text.Length != 0){
                onUserRequest?.Invoke(_InputField.text); // if onUserRequest not null then invoke
                _InputField.text = "";
                _InputField.DeactivateInputField();
            }
        }
    }

    private void OnEnable() {
        Logic.onNotify += ActivateIField; //Subscribe to onNotify action, is triggered when onNotify.Invoke
        Logic.onEscapePressed += HandlePInputField;
    }

    private void HandlePInputField()
    {
        if(_InputField.gameObject.activeSelf)
            _InputField.gameObject.SetActive(false);
        else
            _InputField.gameObject.SetActive(true);
    }

    private void OnDisable(){
        Logic.onNotify -= ActivateIField;
        Logic.onEscapePressed -= HandlePInputField;
    }

    public void ActivateIField(){
        _InputField.ActivateInputField();
        _InputField.Select();
    }

    public void ShowMicrophone(){
        image_mic.gameObject.SetActive(true);
    }

    public void HideMicrophone()
    {
        image_mic.gameObject.SetActive(false);
    }



}