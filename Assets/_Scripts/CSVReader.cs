using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using CsvHelper;
using System.IO;
using System.Linq;
using System.Collections.Specialized;

public class CSVReader : MonoBehaviour
{
    [SerializeField]
    private TextAsset _csv;
    [SerializeField]
    private TextAsset _csvPrediction;
    private OrderedDictionary _dicBsValTarget; //Dictionary of [Blendshape, lists of values]
    private OrderedDictionary _dicBsValPred;//Same as above but for predictions
    private List<float[]> _valTarget;
    private List<float[]> _valPred;

    [SerializeField]
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    private int _nbFrames = 0;
    private int _currentFrame = 0;
    private bool _isAnimating = false;
    private List<string> _blendshapeNames;
    private void Awake() {
        skinnedMesh = skinnedMeshRenderer.sharedMesh;
        _blendshapeNames = new List<string>();
        _dicBsValTarget = new OrderedDictionary();
        _dicBsValPred = new OrderedDictionary();
        _valTarget = new List<float[]>();
        _valPred = new List<float[]>();
    }
    void Start()
    {
        Application.targetFrameRate=60;
        ReadBSCsv(_csv, _dicBsValTarget);
        ReadBSCsv(_csvPrediction, _dicBsValPred);
        GetAllBlendshapeNames();
        ConvertToFloat(_dicBsValPred, _valPred);
        ConvertToFloat(_dicBsValTarget, _valTarget);
    }

    void ConvertToFloat(OrderedDictionary dic, List<float[]> listToPopulate)
    {
            for(int i=0; i<_blendshapeNames.Count; i++)
            {
                string bs = _blendshapeNames[i];
                List<string> listOfValues = (List<string>)dic[bs];
                if(listOfValues == null){
                    float[] zerovalues = Enumerable.Repeat(0.0f, _nbFrames).ToArray();
                    listToPopulate.Add(zerovalues);
                    continue;
                }
                var floatArray = listOfValues.Select(val => float.Parse(val, CultureInfo.InvariantCulture)).ToArray();
                listToPopulate.Add(floatArray);
            }
    }

    private void ReadBSCsv(TextAsset csv, OrderedDictionary dicToPopulate)
    {
        _nbFrames = 0;
        string path = "Assets/" + csv.name + ".csv";
        using var streamReader = new StreamReader(path);
        using var csvReader = new CsvReader(streamReader, CultureInfo.CurrentCulture);
        string value;
        csvReader.Read(); //One time for the header
        InitBsDictionary(csvReader, dicToPopulate);
        //Populate the dictionary
        while (csvReader.Read())
        {
            for (int i = 0; csvReader.TryGetField<string>(i, out value); i++)
            {
                List<string> l = (List<string>)dicToPopulate[i];
                l.Add(value);
            }
            _nbFrames++;

        }
    }

    private void Update() {
        //:TODO: get the recording working with coroutine initialize and stopRecording
        if(Input.GetKeyDown(KeyCode.KeypadEnter) && !_isAnimating)
            StartCoroutine(StartAnimation(_valTarget, "_target")); 
                 
        if(Input.GetKeyDown(KeyCode.KeypadPlus) && !_isAnimating)
            StartCoroutine(StartAnimation(_valPred, "_pred"));
    }

    private IEnumerator StartAnimation(List<float[]> listOfBsWeightsOrdered, string type)
    {
        _isAnimating = true;
        Debug.Log("starting animation....");
        float start = Time.realtimeSinceStartup;
        while(_currentFrame < _nbFrames)
        {
            for(int i=0; i<listOfBsWeightsOrdered.Count ; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, listOfBsWeightsOrdered[i][_currentFrame]);
            }
            _currentFrame++;
            yield return new WaitForEndOfFrame(); //Makes it wait one frame
        }
        _currentFrame = 0;
        _isAnimating = false;
        Debug.Log("Ending animation.");
        float duration = Time.realtimeSinceStartup - start;
        Debug.Log("duration: " + duration);
    }

    void InitBsDictionary(CsvReader reader, OrderedDictionary myDic)
    ///
    /// Creates a dictionary where each of the key is a a header in the csv
    /// everything is string and lowercase, because it's easy to transform string to int or float or anything
    /// 
    {
        string value;
            for (int i = 0; reader.TryGetField<string>(i, out value); i++)
            {
                myDic.Add(value.ToLowerInvariant(), new List<string>());
            }
    }

    void GetAllBlendshapeNames()
    {
        for(int i=0; i<skinnedMesh.blendShapeCount; i++)
        {
            string bs = skinnedMesh.GetBlendShapeName(i).ToLowerInvariant();
            _blendshapeNames.Add(bs);
        }
    }


}
