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
    [SerializeField] Button generateMeshButton;
    [SerializeField] Canvas informationCanvas;
    [SerializeField] Camera aRCamera;
    [SerializeField] int bounds = 0;
    [SerializeField] Text[] informationTexts;

    private Location deviceLocation;
    private Location closestPoint;
    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private WallPlacement wallPlacement;
    private List<Location> withinRadiusData;
    private List<Location> entireCSVData;
    private float offset = 0f;
    private bool meshGenerated = false;

    private void Awake()
    {
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
        wallPlacement = GetComponent<WallPlacement>();
        entireCSVData = CSV_extended.ParseCsvFileUsingResources(pathToCSV);
        generateMeshButton.interactable = false;
        informationCanvas.enabled = false;
        DisableInformationText();
    }

    private void Start()
    {
        SSTools.ShowMessage("Scan the ground", SSTools.Position.top, SSTools.Time.threeSecond);
    }

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
        // Run the coroutine every 2 seconds
        var wait = new WaitForSecondsRealtime(2.0f);

        while (true)
        {
            var distanceFromOrigo = aRCamera.transform.position.magnitude;

            if(distanceFromOrigo > bounds)
            {
                SSTools.ShowMessage("Out of bounds. Reloading", SSTools.Position.top, SSTools.Time.twoSecond);
                new WaitForSecondsRealtime(2f);
                SceneManager.LoadScene("MainScene");
            }
            //Debug.Log("Distance from origo: " + distanceFromOrigo);
            yield return wait;
        }
    }

    public void OnLocationProviderEnabled(LocationReading reading)
    {
        deviceLocation = reading.ToLocation();
        InitializeWaterMesh();
        closestPoint = CSV_extended.ClosestPointGPS(withinRadiusData, deviceLocation);

    }

    public void OnLocationUpdated(LocationReading reading)
    {
        deviceLocation = reading.ToLocation();

        if(!meshGenerated && wallPlacement.IsGroundPlaneSet())
        {
            generateMeshButton.interactable = true;
            meshGenerated = true;

            var stateData = waterMesh.GetLocationsStateData();
            var globalLocalPositions = stateData.GetGlobalLocalPosition();
            wallPlacement.SetCurrentGlobalLocalPositions(globalLocalPositions);
            wallPlacement.SetPointsWithinRadius(withinRadiusData);
            Debug.Log($"Size of points within radius: {withinRadiusData.Count}");

        }
    }

    private void InitializeWaterMesh()
    {        
        withinRadiusData = CSV_extended.PointsWithinRadius(entireCSVData, radius, deviceLocation);
        waterMesh.SetPositionsToHandleLocations(withinRadiusData);
    }

    public void GenerateMesh()
    {
        // Toast instruction
        SSTools.ShowMessage("Place walls if needed", SSTools.Position.top, SSTools.Time.threeSecond);

        double currentWaterHeight = 0;
        var groundPlaneTransform = wallPlacement.GetGroundPlaneTransform();
        var stateData = waterMesh.GetLocationsStateData();
        var globalLocalPositions = stateData.GetGlobalLocalPosition();
        var points = new List<Vector3>();
        float heightAtCamera = (float)closestPoint.Height;

        foreach (var globalLocalPosition in globalLocalPositions)
        {
            float calculatedHeight = 0;
            float latitude = globalLocalPosition.localLocation.z;
            float longitude = globalLocalPosition.localLocation.x;
            Location location = globalLocalPosition.location;
            float height = (float)location.Height;
            float waterHeight = (float)location.WaterHeight;
            bool insideBuilding = location.Building;
            float nearestNeighborHeight = (float)location.NearestNeighborHeight;
            float nearestNeighborWater = (float)location.NearestNeighborWater;

            if (insideBuilding)
            {
                if (nearestNeighborHeight != -9999)
                {
                    calculatedHeight = CalculateRelativeHeight(heightAtCamera, nearestNeighborHeight, nearestNeighborWater);
                    currentWaterHeight = nearestNeighborWater;
                }
            }
            else
            {
                calculatedHeight = CalculateRelativeHeight(heightAtCamera, height, waterHeight);
                currentWaterHeight = waterHeight;
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

        // Enable wall placement and update current water height at closest point
        wallPlacement.SetWallPlacementEnabled(true);
        wallPlacement.WaterMeshGenerated(true);
        wallPlacement.SetCurrentWaterHeight(currentWaterHeight);
        
    }

    private float CalculateRelativeHeight(float heightAtCamera, float heightAtPoint, float waterHeightAtPoint)
    {
        float relativeHeight = heightAtPoint - heightAtCamera + waterHeightAtPoint + offset;
        //Debug.Log($"heightAtCamera {heightAtCamera} heightAtPoint {heightAtPoint} waterHeightAtPoint {waterHeightAtPoint}. Relative height {relativeHeight}");
        return relativeHeight;
    }

    #region UI

    private void DisableInformationText()
    {
        foreach(var text in informationTexts)
        {
            text.enabled = false;
        }
    }

    // Show/hide information panel
    public void ToggleInformation()
    {
        if(informationCanvas.enabled)
        {
            informationCanvas.enabled = false;

            if(!wallPlacement.IsGroundPlaneSet())
            {
                SSTools.ShowMessage("Scan the ground", SSTools.Position.top, SSTools.Time.threeSecond);
            }
        }
        else
        {
            informationCanvas.enabled = true;
        }

        DisableInformationText();
        informationTexts[0].enabled = true;

    }

    // Display next information
    public void NextText()
    {
        var index = 0;
        var length = informationTexts.Length;
        foreach (var text in informationTexts)
        {
            index++;
            if(text.enabled & index < length)
            { 
                text.enabled = false;
                break;
            }
        }
        if(index < length)
        {
            informationTexts[index].enabled = true;
        }
    }

    // Exaggerate height of water
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

    public void ReLoadScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    #endregion
}