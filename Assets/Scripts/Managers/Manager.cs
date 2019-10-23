using ARLocation;
using System;
using System.Collections;
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
    private List<Location> withinRadiusData;
    //private Transform groundPlaneTransform;

    private void Awake()
    {
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
        wallPlacement = GetComponent<WallPlacement>();
        InitializeWaterMesh(pathToWaterCsv);
    }

    // https://docs.unity3d.com/ScriptReference/LocationService.Start.html
    // Isn't continually updated but could be later on if needed. Now it get the phones location at start-up and uses that location 
    // once the "Generate Mesh" button is pressed. Obviuouse improvements can be made here.
    IEnumerator Start()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            //deviceLocation = new Location(Input.location.lastData.latitude, Input.location.lastData.longitude, 0);
        }

        // Stop service if there is no need to query location updates continuously
        // should this stop? Not sure how it is activated in other parts of the code
        Input.location.Stop();
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
            sliderValue += 0.5f;
            sliderValue *= 1.5f;
            var logHeight = Mathf.Log(sliderValue);
            delaunayMesh.SetHeightToMesh(logHeight);
        }
    }

    private void InitializeWaterMesh(string path)
    {        
        waterMesh.enabled = true;
        var fullData = CSV_extended.ParseCsvFileUsingResources(path);

        // Use the device location instead funrther down the line
        //var longitude = 13.200226;
        //var latitude = 55.708675;

        Debug.Log($"Before: {fullData.Count}");
        withinRadiusData = CSV_extended.PointsWithinRadius(fullData, radius, deviceLocation);
        Debug.Log($"After: {withinRadiusData.Count}");

        // Recalculate the height of each vertices before sending it to the waterMeshClass
        waterMesh.SetPositionsToHandleLocations(withinRadiusData);
        HideMesh();

        waterMesh.PositionsUpdated += OnPositionsUpdated;
    }

    // TODO all of this should most likely update continously
    public void GenerateMesh()
    {
        //Debug.Log("GenerateMesh");
        //Debug.Log("Device location: " + deviceLocation);
        var groundPlaneTransform = wallPlacement.GetGroundPlaneTransform();
        var heightOfCamera = groundPlaneTransform.position.y;

        var stateData = waterMesh.GetLocationsStateData();
        var locations = stateData.GetLocalLocations();
        var points = new List<Vector3>();

        var closestPoint = CSV_extended.ClosestPoint(withinRadiusData, deviceLocation);
        float heightAtCamera = (float)closestPoint.Height;


        foreach (var globalLocalPosition in locations)
        {
            // Sets all building heights to 0 atm
            float calculatedHeight = 0;

            float latitude = globalLocalPosition.localLocation.z;
            float longitude = globalLocalPosition.localLocation.x;

            var location = globalLocalPosition.location;
            float heightPoint = (float)location.Height;
            float waterHeight = (float)location.WaterHeight;
            bool insideBuilding = location.Building;
            float nearestNeighborHeight = (float)location.NearestNeighborHeight;
            float nearestNeighborWater = (float)location.NearestNeighborWater;
            //Debug.Log($"water height: {waterHeight}");
            //Debug.Log($"height: {height}");
            //Debug.Log($"Inside building: {insideBuilding}");
            //Debug.Log($"nnh: {nearestNeighborHeight}");
            //Debug.Log($"nnw: {nearestNeighborWater}");

            if (insideBuilding)
            {
                // TODO
                // havn't implemented the logic yet
            }
            else
            {
                // Possible addon hc: heightOfCamera. Not sure but maybe
                calculatedHeight = CalculateRelativeHeight(heightAtCamera, heightPoint, waterHeight);
            }

            Debug.Log($"Calc height: {calculatedHeight} insideBuilding: {insideBuilding}");

            // An exaggeration can be added to calculatedHeight (* 15f) to see the differences more clearly.
            //points.Add(new Vector3(longitude, calculatedHeight, latitude));
            points.Add(new Vector3(longitude, calculatedHeight * 15f, latitude));
        }

        if (groundPlaneTransform != null)
        {
            delaunayMesh.Generate(points, groundPlaneTransform);
        }
        else
        {
            //Debug.Log($"No groundPlaneTransform");
            delaunayMesh.Generate(points, transform);
        }
    }

    private float CalculateRelativeHeight(float heightAtCamera, float heightAtPoint, float waterHeightAtPoint)
    {
        float relativeHeight = heightAtPoint - heightAtCamera + waterHeightAtPoint;
        return relativeHeight;
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