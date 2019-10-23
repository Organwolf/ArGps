using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] string csvPath = "csv_extended";
    [SerializeField] double radius = 2000.0;

    private DelaunayMesh meshGenerator;

    private void Start()
    {
        meshGenerator = GetComponent<DelaunayMesh>();
        GenerateMesh(csvPath);
    }

    private void GenerateMesh(string pathToWaterCsv)
    {
        if (string.IsNullOrEmpty(pathToWaterCsv)) return;

        var data = CSV_extended.ParseCsvFileUsingResources(pathToWaterCsv);
        var longitude = 13.200226;
        var latitude = 55.708675;
        data = CSV_extended.PointsWithinRadius(data, radius, longitude, latitude);


        // Det är inte abstraktionen vi la till då? jo detta
        // Samma
        double multipler = 100;
        var meshData = new List<Vector3>();
        foreach (var location in data)
        {
            float x = (float)(location.Longitude * multipler);
            float y = (float)(location.Altitude * multipler);
            float z = (float)(location.Latitude * multipler);
            meshData.Add(new Vector3(x, y, z));
        }
        meshGenerator.Generate(meshData, transform); 
    }
}
