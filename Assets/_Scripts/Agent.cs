using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[System.Serializable]
public class UnityStringEvent : UnityEvent<string> {}

public class Agent : MonoBehaviour
{
    public UnityStringEvent onAgentEvent;//Event triggering ChatGPTConversation, events subscribe from Unity Editor
    public static Action<string> onAnswer;//Event triggered after getting answer from GPT API, Invoke event from Logic
    
    [TextArea(4, 6)]
    public string InitialPrompt;
    public float responseDelay;

    public void OnAgentEnabled(string message){
        Debug.Log("sending request...");
        onAgentEvent.Invoke(InitialPrompt + message); //Send initial prompt (from editor) and message from user.
    }

    public void AnswerFromGPT(string answer){
        LogInterface.instance.Log(answer);
        onAnswer?.Invoke(answer);
    }
    
    private void OnEnable() {
        Logic.callAgent += OnAgentEnabled; //Subscribe to callAgent action, is triggered when callAgent.Invoke
    }

    private void OnDisable() {
        Logic.callAgent -= OnAgentEnabled;
    }


}
