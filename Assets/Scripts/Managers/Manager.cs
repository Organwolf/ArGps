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
    }

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
}