using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextBubbleManager : MonoBehaviour
{
    [Header("Speech Settings")]
    public float prefHeightText;
    public float speechDelay;
    [SerializeField]
    private GameObject bubbleImage;
    [SerializeField]
    private Button nextButton;
    private RectTransform rt;
    private float currentTextHeight;
    private float initialHeightImage;
    private float initialHeightText;
    [SerializeField]
    private TextMeshProUGUI text;
    private string response;
    public static bool isWriting;

    // Start is called before the first frame update
    void Start()
    {
        text.text = "";
        bubbleImage.SetActive(false);
        rt = bubbleImage.GetComponent<RectTransform>();
        initialHeightImage = rt.rect.height;
        initialHeightText = text.preferredHeight;
        nextButton.interactable = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)){
            StartCoroutine(StartDialogue());
        }
    }

    private void OnEnable() {
        TextToSpeech.onSpeechProcessingDone += UpdateText;
    }

    private void OnDisable() {
        TextToSpeech.onSpeechProcessingDone -= UpdateText;
    }

    private void UpdateText(string message){
        bubbleImage.SetActive(true);
        response = message;
        StartCoroutine(StartDialogue());

    }

    private IEnumerator StartDialogue(){
        //Wait for the text animation to finish before starting another bubble.
        //Otherwise it messes up characters position
        if(isWriting)
            yield return new WaitUntil(() => !isWriting); 
        isWriting=true;
        currentTextHeight = initialHeightText;
        ResizeImage(rt.sizeDelta.x, initialHeightImage);
        text.text = "";
        foreach(char letter in response.ToCharArray())
        {
            text.text += letter;
            if(text.preferredHeight < prefHeightText)
            {
                if(HeightHasIncreased())
                {
                    currentTextHeight = text.preferredHeight;
                    ResizeImage(rt.sizeDelta.x, rt.sizeDelta.y + 10f);
                }
                yield return new WaitForSeconds(speechDelay);
            }
            else
            {
                text.text = text.text.Remove(text.text.Length -1);
                nextButton.interactable = true;
                break;
            }
        }
        response = response.Replace(text.text, "");
        isWriting=false;
    }

    private bool HeightHasIncreased(){
        return currentTextHeight < text.preferredHeight;
    }

    //Resize the bubble image
    private void ResizeImage(float width, float height){
        rt.sizeDelta = new Vector2(width, height);
    }

    public void NextText() //Used for click button
    {
        nextButton.interactable = false;
        StartCoroutine(StartDialogue());
    }
}
