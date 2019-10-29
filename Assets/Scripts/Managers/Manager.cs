using ARLocation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Manager : MonoBehaviour
{
    [SerializeField] string pathToWaterCsv;
    [SerializeField] double radius = 20.0;
    [SerializeField] Slider exaggerateHeightSlider;    // UI
    [SerializeField] Text togglePlacementText;

    private Location deviceLocation;
    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private WallPlacement wallPlacement;
    private List<Location> withinRadiusData;

    private void Awake()
    {
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
        wallPlacement = GetComponent<WallPlacement>();
        //InitializeWaterMesh(pathToWaterCsv);
    }

    //// https://docs.unity3d.com/ScriptReference/LocationService.Start.html
    //// Isn't continually updated but could be later on if needed. Now it get the phones location at start-up and uses that location 
    //// once the "Generate Mesh" button is pressed. Obviuouse improvements can be made here.
    //IEnumerator Start()
    //{
    //    // First, check if user has location service enabled
    //    if (!Input.location.isEnabledByUser)
    //        yield break;

    //    // Start service before querying location
    //    Input.location.Start();

    //    // Wait until service initializes
    //    int maxWait = 20;
    //    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
    //    {
    //        yield return new WaitForSeconds(1);
    //        maxWait--;
    //    }

    //    // Service didn't initialize in 20 seconds
    //    if (maxWait < 1)
    //    {
    //        print("Timed out");
    //        yield break;
    //    }

    //    // Connection has failed
    //    if (Input.location.status == LocationServiceStatus.Failed)
    //    {
    //        print("Unable to determine device location");
    //        yield break;
    //    }
    //    else
    //    {
    //        // Access granted and location value could be retrieved
    //        print("Start Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
    //        deviceLocation = new Location(Input.location.lastData.latitude, Input.location.lastData.longitude, 0);
    //    }
    //    // Stop service if there is no need to query location updates continuously
    //    // could add a yeild that runs every second or something?
    //    Input.location.Stop();
    //}

    public void OnLocationProviderEnabled(LocationReading reading)
    {
        Debug.Log($"OnLocationProviderEnabled Lat: {reading.latitude} Long: {reading.longitude}.");
        deviceLocation = reading.ToLocation();
        InitializeWaterMesh(pathToWaterCsv);
    }

    private void InitializeWaterMesh(string path)
    {        
        var fullData = CSV_extended.ParseCsvFileUsingResources(path);

        Debug.Log($"Before: {fullData.Count}");
        withinRadiusData = CSV_extended.PointsWithinRadius(fullData, radius, deviceLocation);
        Debug.Log($"After: {withinRadiusData.Count}");

        // Recalculate the height of each vertices before sending it to the waterMeshClass
        waterMesh.SetPositionsToHandleLocations(withinRadiusData);
        //HideMesh();
    }

    // TODO: all of this should most likely update continously
    public void GenerateMesh()
    {
        Debug.Log("Device location: " + deviceLocation);
        var groundPlaneTransform = wallPlacement.GetGroundPlaneTransform();
        
        // currently not used
        // var heightOfCamera = groundPlaneTransform.position.y;

        var stateData = waterMesh.GetLocationsStateData();
        var locations = stateData.GetLocalLocations();
        var points = new List<Vector3>();

        var closestPoint = CSV_extended.ClosestPoint(withinRadiusData, deviceLocation);
        float heightAtCamera = (float)closestPoint.Height;

        foreach (var globalLocalPosition in locations)
        {
            // Sets all value of -9999 to 0 atm - could change logic while reading the csv?
            float calculatedHeight = 0;

            float latitude = globalLocalPosition.localLocation.z;
            float longitude = globalLocalPosition.localLocation.x;
            var location = globalLocalPosition.location;
            float heightPoint = (float)location.Height;
            float waterHeight = (float)location.WaterHeight;
            bool insideBuilding = location.Building;
            float nearestNeighborHeight = (float)location.NearestNeighborHeight;
            float nearestNeighborWater = (float)location.NearestNeighborWater;

            if (insideBuilding)
            {
                if (nearestNeighborHeight != -9999)
                {
                    calculatedHeight = CalculateRelativeHeight(heightAtCamera, nearestNeighborHeight, nearestNeighborWater);
                }
            }
            else
            {
                calculatedHeight = CalculateRelativeHeight(heightAtCamera, heightPoint, waterHeight);
            }

            points.Add(new Vector3(longitude, calculatedHeight, latitude)); // Exaggerate height if needed
            //Debug.Log($"Calc height: {calculatedHeight} insideBuilding: {insideBuilding}");
        }

        if (groundPlaneTransform != null)
        {
            delaunayMesh.Generate(points, groundPlaneTransform);
        }
        else
        {
            delaunayMesh.Generate(points, transform);
        }
    }

    private float CalculateRelativeHeight(float heightAtCamera, float heightAtPoint, float waterHeightAtPoint)
    {
        float relativeHeight = heightAtPoint - heightAtCamera + waterHeightAtPoint;
        return relativeHeight;
    }

    // UI
    public void AlterHeightOfMesh()
    {
        var sliderValue = exaggerateHeightSlider.value;
        if (sliderValue > 0)
        {
            sliderValue += 0.5f;
            sliderValue *= 2f;
            var logHeight = Mathf.Log(sliderValue);
            Debug.Log($"log height: {logHeight}");
            delaunayMesh.SetHeightToMesh(logHeight);
        }
    }

    public void RenderWalls()
    {
        wallPlacement.RenderWalls();
    }

    public void RemovePreviouseWall()
    {
        wallPlacement.RemovePreviousWall();
    }

    public void ToggleWallPlacement()
    {
        wallPlacement.ToggleWallPlacement();
        if (wallPlacement.GetWallPlacementEnabled())
            togglePlacementText.text = "Place Measuring Stick";
        else
        {
            togglePlacementText.text = "Place Walls";
        }
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    //public void ShowMesh()
    //{
    //    SetMeshVisible(true);
    //}

    //public void HideMesh()
    //{
    //    SetMeshVisible(false);
    //}

    //private void SetMeshVisible(bool visible)
    //{
    //    var meshes = waterMesh.GetComponentsInChildren<MeshRenderer>();
    //    foreach (var mesh in meshes)
    //    {
    //        mesh.enabled = visible;
    //    }
    //}

    // Currently doesn't work
    //public void ResetSession()
    //{
    //    //waterMesh.enabled = false;
    //    //waterMesh.Restart();
    //    //delaunayMesh.ClearMesh();
    //    //wallPlacement.ResetSession();

    //    // solve the rescanning of the ground
    //}
}