using ARLocation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Manager : MonoBehaviour
{
    [SerializeField] string pathToCSV;
    [SerializeField] double radius = 20.0;
    [SerializeField] Slider exaggerateHeightSlider;
    [SerializeField] Text togglePlacementText;
    [SerializeField] Button GenerateMeshButton;
    [SerializeField] Camera ARCamera;
    [SerializeField] int bounds = 0;

    private Location deviceLocation;
    private Location closestPoint;
    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private WallPlacement wallPlacement;
    private List<Location> withinRadiusData;
    private List<Location> entireCSVData;
    private float offset = 0f;

    private void Awake()
    {
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
        wallPlacement = GetComponent<WallPlacement>();
        entireCSVData = CSV_extended.ParseCsvFileUsingResources(pathToCSV);
        GenerateMeshButton.interactable = false;

        // To be implemented in settings
        if (PlayerPrefs.HasKey("Radius"))
        {
            radius = PlayerPrefs.GetInt("Radius");
            Debug.Log("Current radius: " + radius);
        }

        if (PlayerPrefs.HasKey("Offset"))
        {
            offset = PlayerPrefs.GetInt("Offset");
            // comvert from cm to m
            offset /= 100f;
            Debug.Log("Current offset: " + offset);
        }
    }

    //private void OnDisable()
    //{
    //    PlayerPrefs.DeleteAll();
    //}

    private UnityEngine.Coroutine updateEachSecond;

    private void OnEnable()
    {
        updateEachSecond = StartCoroutine(OnPlayerOutOfBounds(bounds));
    }

    private void OnDisable()
    {
        StopCoroutine(updateEachSecond);
        updateEachSecond = null;
        Debug.Log("App disabled");
    }

    private IEnumerator OnPlayerOutOfBounds(int bounds)
    {
        // Pauses the coroutine 2 seconds between each execution
        var wait = new WaitForSecondsRealtime(2.0f);

        while (true)
        {
            var distanceFromOrigo = ARCamera.transform.position.magnitude;
            Debug.Log("Distance from origo: " + distanceFromOrigo);
            if(distanceFromOrigo > bounds)
            {
                SSTools.ShowMessage("Out of bounds. Scan ground", SSTools.Position.top, SSTools.Time.twoSecond);
            }
            yield return wait;
        }
    }

    public void OnLocationProviderEnabled(LocationReading reading)
    {
        //Debug.Log($"OnLocationProviderEnabled Lat: {reading.latitude} Long: {reading.longitude}.");
        deviceLocation = reading.ToLocation();
        InitializeWaterMesh();
        closestPoint = CSV_extended.ClosestPoint(withinRadiusData, deviceLocation);
        //Debug.Log("Closest point: " + closestPoint);
    }

    public void OnLocationUpdated(LocationReading reading)
    {
        deviceLocation = reading.ToLocation();
        CSV_extended.ClosestPoint(withinRadiusData, deviceLocation);
    }

    private void InitializeWaterMesh()
    {        
        withinRadiusData = CSV_extended.PointsWithinRadius(entireCSVData, radius, deviceLocation);
        waterMesh.SetPositionsToHandleLocations(withinRadiusData);
        GenerateMeshButton.interactable = true;
    }

    public void GenerateMesh()
    {
        // currently not used -> var heightOfCamera = groundPlaneTransform.position.y;

        var groundPlaneTransform = wallPlacement.GetGroundPlaneTransform();
        var stateData = waterMesh.GetLocationsStateData();

        var globalLocalPositions = stateData.GetGlobalLocalPosition();

        var points = new List<Vector3>();

        //var closestPoint = CSV_extended.ClosestPoint(withinRadiusData, deviceLocation);
        float heightAtCamera = (float)closestPoint.Height;

        // vad är det du gör? OK sorry såhär ligger det till

        //foreach (var globalLocalPosition in globalLocalPositions)
        foreach (var globalLocalPosition in globalLocalPositions)
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
        float relativeHeight = heightAtPoint - heightAtCamera + waterHeightAtPoint + offset;
        //Debug.Log($"heightAtCamera {heightAtCamera} heightAtPoint {heightAtPoint} waterHeightAtPoint {waterHeightAtPoint}. Relative height {relativeHeight}");
        return relativeHeight;
    }

    #region UI
    public void AlterHeightOfMesh()
    {
        var sliderValue = exaggerateHeightSlider.value;
        if (sliderValue > 0)
        {
            sliderValue += 0.5f;
            sliderValue *= 2f;
            var logHeight = Mathf.Log(sliderValue);
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
    #endregion
}