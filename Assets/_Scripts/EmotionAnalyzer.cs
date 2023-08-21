using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EmotionAnalyzer : MonoBehaviour
{
    public UnityStringEvent onEmotionAnalyzerEnabled;
    public static Action<string> onEmotionAnalyzed;
    [TextArea(4, 6)]
    public string InitialPrompt;


    void OnEnable()
    {
        Agent.onAnswer += AnalyzeText;
    }

    void OnDisable()
    {
        Agent.onAnswer -= AnalyzeText;
    }

    public void AnalyzeText(string message)
    {
        onEmotionAnalyzerEnabled?.Invoke(InitialPrompt + message);
    }

    public void EmotionOutput(string answer)
    {
        LogInterface.instance.Log("Emotion: " + answer);
        Debug.Log("Emotion: " + answer);
        onEmotionAnalyzed?.Invoke(answer);
    }

}
