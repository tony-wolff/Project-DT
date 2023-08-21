using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogInterface : MonoBehaviour
{
    //Singleton
    private static LogInterface _instance;
    [SerializeField]
    private TextMeshProUGUI _logText;
    private void Awake() {
        _instance = this;
        _logText.text = "";
    }

    public static LogInterface instance {
        get 
        {
            //ensures that if another awake's class is called before this awake, it finds the instance;
            if (_instance == null) _instance = FindObjectOfType<LogInterface>();
            return _instance;
        }
    }

    public void Log(object s)
    {
        if(_logText.text == "")
            _logText.text += s;
        else
            _logText.text += "\n" + s;
    }
}
