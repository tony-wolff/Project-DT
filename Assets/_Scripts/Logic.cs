using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Diagnostics;

public class Logic : MonoBehaviour
{
    public GameObject canvasText;
    public static Action<string> callAgent; //Event triggered after user enters prompt, invoke methods from Agent

    public static Action onNotify; //Event triggered after Agent API answered, invoke method from InputManager
    public static Action onEscapePressed;
    public static bool isPrompting=true;
    [SerializeField]
    private NNModel _mobilenet;
    [SerializeField]
    private Texture2D _imageToRecognise;
    private static Model _runtimeModel;
    private bool isSpeaking=false;

    public UnityEvent onSpeechStart;
    public UnityEvent onSpeechEnd;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            HandleCanvas();
        }
        if(Input.GetKeyDown(KeyCode.Space) && !isPrompting){
            HandleSpeechActivation();
        }
    }

    private void CallAgentRequest(string prompt){
        LogInterface.instance.Log("user requested prompt: " + prompt);
        callAgent?.Invoke(prompt);
    }

    //Similar to Start or Awake, but called everytime the object is enabled
    private void OnEnable() {
        InputManager.onUserRequest += CallAgentRequest; //Subscribe to onUserRequest action, is triggered when onUserRequest.Invoke
        Agent.onAnswer += NotifyInputManger; //Subscribe to onAnswer action, is triggered when onAnswer.Invoke
        SpeechRecognition.onSpeechResult += CallAgentRequest;
        ControllerInputManager.onGripPressed += HandleCanvas; //gripButton enables/disables prompt canvas
        ControllerInputManager.onSecondaryPress += HandleSpeechActivation;//left secondary button enables/disable speech recognition
    }

    private void OnDisable() {
        InputManager.onUserRequest -= CallAgentRequest;
        Agent.onAnswer -= NotifyInputManger;
        SpeechRecognition.onSpeechResult -= CallAgentRequest;
        ControllerInputManager.onGripPressed -= HandleCanvas;
        ControllerInputManager.onSecondaryPress -= HandleSpeechActivation;
    }
    
    //message is not used, avoid creating another action event
    private void NotifyInputManger(string message){
        onNotify?.Invoke();
    }

    private void HandleCanvas(){
            if(isPrompting)
            {
                onEscapePressed?.Invoke();
                isPrompting=false;
            }
            else
            {
                onEscapePressed?.Invoke();
                isPrompting=true;
                NotifyInputManger("");
            }
    }

    private void HandleSpeechActivation()
    {
        if(!isSpeaking){
            onSpeechStart.Invoke();
            isSpeaking=true;
        }
        else{
            onSpeechEnd.Invoke();
            isSpeaking=false;
        }
    }
    private void Awake() {
        
    }

    private void Start() {
        _runtimeModel = ModelLoader.Load(_mobilenet);

    }

    public static void Inference(Texture2D image)
    {
        Stopwatch st = new Stopwatch();
        IWorker engine = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel);
        using (var input = new Tensor(image, channels: 3))
        {
          // execute neural network with specific input and get results back
          var output = engine.Execute(input).PeekOutput();
          LogInterface.instance.Log("Idx: " + output.ArgMax()[0]);
        }
        engine.Dispose();
    }
}
