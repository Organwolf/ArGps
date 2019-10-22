using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Thoughts: does this even need to be a monobehaviour?
 * 
 */

public class CSV_extended : MonoBehaviour
{
    [SerializeField] private string pathToFile;

    private List<float[]> parsedData = new List<float[]>(); 

    private void Start()
    {
        ParseCsvFileUsingResources(pathToFile);
    }

    public void ParseCsvFileUsingResources(string pathToFile)
    {
        TextAsset file = Resources.Load(pathToFile) as TextAsset;
        string[] lines = file.text.Split('\n');

        foreach(string line in lines)
        {
            string[] split = line.Split(',');

            float[] parsedLine = new float[]
            {
                float.Parse(split[0]),
                float.Parse(split[1]),
                float.Parse(split[2]),
                float.Parse(split[3]),
                float.Parse(split[4]),
                float.Parse(split[5]),
                float.Parse(split[6])
            };
            parsedData.Add(parsedLine);
        }
    }
}
