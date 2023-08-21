//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//
// <code>
using System;
using System.Threading;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TextToSpeech : MonoBehaviour
{
    // Hook up the three properties below with a Text, InputField and Button object in your UI.
    public AudioSource audioSource;

    // Replace with your own subscription key and service region (e.g., "westus").
    [SerializeField]
    private string SubscriptionKey = "2bc99d2a42e3487da6cd607687e8fd3f";
    [SerializeField]
    private string Region = "japaneast";

    private const int SampleRate = 24000;

    private object threadLocker = new object();
    private bool waitingForSpeak;
    private bool audioSourceNeedStop;
    private string message;
    private string _emotion;
    private string _answerFromLLM;
    private SpeechConfig speechConfig;
    private SpeechSynthesizer synthesizer;
    public static Action<string>onSpeechProcessingDone;

    public async Task TTS(string utterance)
    {

        string newMessage = null;
        var startTime = DateTime.Now;
        message = utterance;
        // Starts speech synthesis, and returns once the synthesis is started.
        var element = XElement.Parse($"<root>{utterance}</root>"); // message
        var ssml = GenerateSsml("en-US", "Female", "en-US-AriaNeural", _emotion, "2", element.Nodes());//

        using (var result = await synthesizer.StartSpeakingSsmlAsync(ssml)) //"Using" ensures that the code is disposed after exiting the scope of the block
        {
            // Native playback is not supported on Unity yet (currently only supported on Windows/Linux Desktop).
            // Use the Unity API to play audio here as a short term solution.
            // Native playback support will be added in the future release.
            var audioDataStream = AudioDataStream.FromResult(result);
            var isFirstAudioChunk = true;
            var audioClip = AudioClip.Create(
                "Speech",
                SampleRate * 600, // Can speak 10mins audio as maximum
                1,
                SampleRate,
                true,
                (float[] audioChunk) =>
                {
                    var chunkSize = audioChunk.Length;
                    var audioChunkBytes = new byte[chunkSize * 2];
                    var readBytes = audioDataStream.ReadData(audioChunkBytes);
                    if (isFirstAudioChunk && readBytes > 0)
                    {
                        var endTime = DateTime.Now;
                        var latency = endTime.Subtract(startTime).TotalMilliseconds;
                        newMessage = $"Speech synthesis succeeded!\nLatency: {latency} ms.";
                        isFirstAudioChunk = false;
                    }

                    for (int i = 0; i < chunkSize; ++i)
                    {
                        if (i < readBytes / 2)
                        {
                            audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                        }
                        else
                        {
                            audioChunk[i] = 0.0f;
                        }
                    }

                    if (readBytes == 0)
                    {
                        Thread.Sleep(200); // Leave some time for the audioSource to finish playback
                        audioSourceNeedStop = true;
                    }
                });

            audioSource.clip = audioClip;
            audioSource.Play();
        }

        lock (threadLocker)
        {
            if (newMessage != null)
            {
                message = newMessage;
            }

            waitingForSpeak = false;
        }
        await Task.Yield();
    }

    void Start()
    {
        speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);
        // The default format is RIFF, which has a riff header.
        // We are playing the audio in memory as audio clip, which doesn't require riff header.
        // So we need to set the format to raw (24KHz for better quality).
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);
        // Creates a speech synthesizer.
        // Make sure to dispose the synthesizer after use!
        synthesizer = new SpeechSynthesizer(speechConfig, null);

        synthesizer.SynthesisCanceled += (s, e) =>
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
            message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
            Debug.Log(cancellation.Reason);
            Debug.Log(cancellation.ErrorDetails);
        };
    }



    void OnEnable()
    {
        Agent.onAnswer += onSpeech;
        EmotionAnalyzer.onEmotionAnalyzed += onEmotionDetected;
    }

    async void onEmotionDetected(string emotion)
    {
        this._emotion = emotion;
        await TTS(_answerFromLLM);
        onSpeechProcessingDone?.Invoke(_answerFromLLM);
    }

    void OnDisable()
    {
        Agent.onAnswer -= onSpeech;
        EmotionAnalyzer.onEmotionAnalyzed -= onEmotionDetected;
    }

    private void onSpeech(string message)
    {
        _answerFromLLM = message;
    }

    void Update()
    {
        lock (threadLocker)
        {
            if (audioSourceNeedStop)
            {
                audioSource.Stop();
                audioSourceNeedStop = false;
            }
        }
    }

    void OnDestroy()
    {
        if (synthesizer != null)
        {
            synthesizer.Dispose();
        }
    }

    private string GenerateSsml(string locale, string gender, string name, string style, string styledegree, IEnumerable<XNode> nodes)
    {
        XNamespace mstts = "https://www.w3.org/2001/mstts";
        var ssmlDoc = new XDocument(
                        new XElement("speak",
                            new XAttribute("version", "1.0"),
                            new XAttribute(XNamespace.Xmlns + "mstts", "https://www.w3.org/2001/mstts"),
                            new XAttribute(XNamespace.Xml + "lang", "en-US"),
                            new XElement("voice",
                                new XAttribute(XNamespace.Xml + "lang", locale),
                                new XAttribute(XNamespace.Xml + "gender", gender),
                                new XAttribute("name", name),
                                new XElement( mstts + "express-as",
                                    new XAttribute("style", style),
                                    new XAttribute("styledegree", styledegree)),
                                nodes)));
        return ssmlDoc.ToString();
    }
}
// </code>
