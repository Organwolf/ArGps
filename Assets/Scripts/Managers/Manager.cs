using ARLocation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static WaterMesh;


public class Manager : MonoBehaviour
{
    [SerializeField] string pathToWaterCsv;
    [SerializeField] double radius = 20.0;
    [SerializeField] Location deviceLocation;

    // UI
    [SerializeField] Slider exaggerateHeightSlider;

    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private WallPlacement wallPlacement;
    private ARLocationProvider locationProvider;
    //private Transform groundPlaneTransform;

    private void Awake()
    {
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
        wallPlacement = GetComponent<WallPlacement>();
    }
   
    private void Start()
    {
        InitializeWaterMesh(pathToWaterCsv);
    }

    // Called each time the positions update
    private void OnPositionsUpdated(LocationsStateData stateData)
    {
        Debug.Log("OnPositionsUpdated");
        //var locations = stateData.GetLocalLocations();
        //var face = stateData.getLocalLocations();
        //var points = new List<Vector3>();
        //foreach (var globalLocalPosition in locations)
        //{
        //    var location = globalLocalPosition.location;
        //    float longitude = (float)location.Longitude;
        //    float altitude = (float)location.Altitude;
        //    float latitude = (float)location.Latitude;
        //    points.Add(new Vector3(longitude, altitude, latitude));
        //}
        
        //if (groundPlaneTransform != null)
        //{
        //    delaunayMesh.Generate(points, groundPlaneTransform);
        //}
    }
    
    // UI
    public void AlterHeightOfMesh()
    {
        var sliderValue = exaggerateHeightSlider.value;
        if(sliderValue > 0)
        {
            sliderValue *= 2;
            sliderValue = Mathf.Log(sliderValue);
            delaunayMesh.SetHeightToMesh(sliderValue);
        }
    }

    private void InitializeWaterMesh(string path)
    {
        // Send the ground plane transform to the delaunayMesh class Yes det skriver ut generate mesh
        
        waterMesh.enabled = true;
        var data = CSV_extended.ParseCsvFileUsingResources(path);
        var longitude = 13.200226;
        var latitude = 55.708675;
        Debug.Log($"Before: {data.Count}");
        data = CSV_extended.PointsWithinRadius(data, radius, longitude, latitude);
        Debug.Log($"After: {data.Count}");
        waterMesh.SetPositionsToHandleLocations(data);
        HideMesh();

        waterMesh.PositionsUpdated += OnPositionsUpdated;
    }

    // Då testar jag det

    public void GenerateMesh()
    {
        Debug.Log("GenerateMesh");

        var stateData = waterMesh.GetLocationsStateData();
        var locations = stateData.GetLocalLocations();
        var points = new List<Vector3>();
        
        foreach (var globalLocalPosition in locations)
        {
            var location = globalLocalPosition.location;
            float longitude = globalLocalPosition.localLocation.x;
            float height = (float)location.Height;
            float waterHeight = (float)location.WaterHeight;
            float latitude = globalLocalPosition.localLocation.z;

            Debug.Log($"water height: {waterHeight}");
            Debug.Log($"height: {height}");

            //var calculatedHeight = height + (waterHeight / 10f);
            var calculatedHeight = (waterHeight / 10f);
            points.Add(new Vector3(longitude, calculatedHeight, latitude));
        }

        var groundPlaneTransform = wallPlacement.GetGroundPlaneTransform();
        if (groundPlaneTransform != null)
        {
            delaunayMesh.Generate(points, groundPlaneTransform);
        }
        else
        {
            Debug.Log($"No groundPlaneTransform");
            delaunayMesh.Generate(points, transform);
        }
    }

    // Använd dessa i GUIt
    public void ShowMesh()
    {
        SetMeshVisible(true);
    }

    public void HideMesh()
    {
        SetMeshVisible(false);
    }

    private void SetMeshVisible(bool visible)
    {
        var meshes = waterMesh.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in meshes)
        {
            mesh.enabled = visible;
        }
    }

    // Currently doesn't work
    public void ResetSession()
    {
        //waterMesh.enabled = false;
        //waterMesh.Restart();
        //delaunayMesh.ClearMesh();
        //wallPlacement.ResetSession();

        // solve the rescanning of the ground
    }
}