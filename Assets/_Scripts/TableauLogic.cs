using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TableauLogic : MonoBehaviour
{
    public List<Texture2D> pic_list;
    [SerializeField]
    private Transform _tableau;
    private int _listIndex=0;
    private void OnEnable() {
        TouchButton.OnLeftPress += LeftPress;
        TouchButton.OnRightPress += RightPress;
        TouchButton.OnValidatePress += ValidatePress;
        TouchButton.OnTestPress += TestPress;
    }

    private void OnDisable(){
        TouchButton.OnLeftPress -= LeftPress;
        TouchButton.OnRightPress -= RightPress;
        TouchButton.OnValidatePress -= ValidatePress;
        TouchButton.OnTestPress -= TestPress;
    }

    private void ValidatePress()
    {
        UnityEngine.Debug.Log("validate");
        Material m = _tableau.GetComponent<Renderer>().material;
        Texture2D image = m.mainTexture as Texture2D;
        Stopwatch st = new Stopwatch();
        st.Start();
        Logic.Inference(image);
        st.Stop();
        LogInterface.instance.Log("index: " + _listIndex);
        LogInterface.instance.Log("ms: " + st.ElapsedMilliseconds);
    }

    private void RightPress()
    {
        UnityEngine.Debug.Log("right press");
        if(_listIndex+1 >= pic_list.Count)
            return;
        Material m = _tableau.GetComponent<Renderer>().material;
        m.mainTexture = pic_list[_listIndex+1];
        _listIndex++;
    }

    private void LeftPress()
    {
        UnityEngine.Debug.Log("left press");
        if(_listIndex == 0)
            return;
        Material m = _tableau.GetComponent<Renderer>().material;
        m.mainTexture = pic_list[_listIndex-1];
        _listIndex--;
    }

    private void TestPress()
    {
        List<float> inferenceTime = new List<float>();
        Stopwatch st = new Stopwatch();
        foreach (var image in pic_list)
        {
            st.Start();
            Logic.Inference(image);
            st.Stop();
            inferenceTime.Add(st.ElapsedMilliseconds);
            st.Reset();

        }
        float mean = Mean(inferenceTime);
        float variance = Variance(inferenceTime);
        LogInterface.instance.Log("mean: " + mean);
        LogInterface.instance.Log("variance" + variance);
        LogInterface.instance.Log("values: ");
        string vals="";
        inferenceTime.ForEach(x => vals += x + ", ");
        LogInterface.instance.Log(vals);
    }

    private float Mean(List<float> list)
    {
        float m = 0;
        list.ForEach(x => m+=x);
        return (m/list.Count);
    }

    private float Variance(List<float> list)
    {
        float variance=0;
        float m = Mean(list);
        foreach (var v in list)
        {
            variance += (v-m)*(v-m);

        }
        return (variance/list.Count);
    }
}
