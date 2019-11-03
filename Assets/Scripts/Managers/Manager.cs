using ARLocation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class Manager : MonoBehaviour
{
    [SerializeField] string pathToCSV;
    [SerializeField] double radius = 20.0;
    [SerializeField] Slider exaggerateHeightSlider;
    [SerializeField] Text togglePlacementText;
    [SerializeField] Button generateMeshButton;
    [SerializeField] Button informationButton;
    [SerializeField] Canvas informationCanvas;
    [SerializeField] Camera aRCamera;
    [SerializeField] ARSession aRSession;
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

    private void Awake()
    {
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
        wallPlacement = GetComponent<WallPlacement>();
        entireCSVData = CSV_extended.ParseCsvFileUsingResources(pathToCSV);
        generateMeshButton.interactable = false;

        informationCanvas.enabled = false;
        DisableInformationText();

        // Playerprefs -> not implemented correctly yet
        //if (PlayerPrefs.HasKey("Radius"))
        //{
        //    radius = PlayerPrefs.GetInt("Radius");
        //    Debug.Log("Current radius: " + radius);
        //}

        //if (PlayerPrefs.HasKey("Offset"))
        //{
        //    offset = PlayerPrefs.GetInt("Offset");
        //    // comvert from cm to m
        //    offset /= 100f;
        //    Debug.Log("Current offset: " + offset);
        //}
    }

    private void Start()
    {
        // Only activate when info panel ånot avtive
        //SSTools.ShowMessage("Scan the ground", SSTools.Position.top, SSTools.Time.threeSecond);
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
        // Pauses the coroutine 2 seconds between each execution
        var wait = new WaitForSecondsRealtime(2.0f);

        while (true)
        {
            var distanceFromOrigo = aRCamera.transform.position.magnitude;
            Debug.Log("Distance from origo: " + distanceFromOrigo);
            if(distanceFromOrigo > bounds)
            {
                SSTools.ShowMessage("Out of bounds. Reloading", SSTools.Position.top, SSTools.Time.twoSecond);
                new WaitForSecondsRealtime(2f);
                SceneManager.LoadScene("MainScene");

                /* 
                 * trigger a function that: 
                 *  - destroys prev.planes
                 *  - prompt user to scan (1 sek msges?)
                 *  - walk through the process
                 *  - remove att walls with object connected to them
                 *  - remove the previouse mesh
                 */

                // fade out fade in? animation?

                //wallPlacement.ResetSession();
                //wallPlacement.TogglePlaneDetection();
                //generateMeshButton.interactable = false;
                //waterMesh.Restart();
            }
            yield return wait;
        }
    }

    public void OnLocationProviderEnabled(LocationReading reading)
    {
        deviceLocation = reading.ToLocation();
        InitializeWaterMesh();
        closestPoint = CSV_extended.ClosestPoint(withinRadiusData, deviceLocation);
        // only enable button when then groundplane is placed
    }

    public void OnLocationUpdated(LocationReading reading)
    {
        deviceLocation = reading.ToLocation();
        // was only a test
        //CSV_extended.ClosestPoint(withinRadiusData, deviceLocation);
        if(wallPlacement.IsGroundPlaneSet())
        {
            generateMeshButton.interactable = true;
        }
    }

    private void InitializeWaterMesh()
    {        
        withinRadiusData = CSV_extended.PointsWithinRadius(entireCSVData, radius, deviceLocation);
        waterMesh.SetPositionsToHandleLocations(withinRadiusData);
    }

    public void GenerateMesh()
    {
        var groundPlaneTransform = wallPlacement.GetGroundPlaneTransform();
        var stateData = waterMesh.GetLocationsStateData();
        var globalLocalPositions = stateData.GetGlobalLocalPosition();
        var points = new List<Vector3>();
        float heightAtCamera = (float)closestPoint.Height;


        foreach (var globalLocalPosition in globalLocalPositions)
        {
            // Sets all value of -9999 to 0 atm - could change logic while reading the csv?
            float calculatedHeight = 0;
            float latitude = globalLocalPosition.localLocation.z;
            float longitude = globalLocalPosition.localLocation.x;
            var location = globalLocalPosition.location;
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
                }
            }
            // point not inside a building
            else
            {
                calculatedHeight = CalculateRelativeHeight(heightAtCamera, height, waterHeight);
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

    private void DisableInformationText()
    {
        foreach(var text in informationTexts)
        {
            text.enabled = false;
        }
    }

    public void ToggleInformation()
    {
        // Show/hide information panel
        if(informationCanvas.enabled)
        {
            SSTools.ShowMessage("Scan the ground", SSTools.Position.top, SSTools.Time.threeSecond);
            informationCanvas.enabled = false;
        }
        else
        {
            informationCanvas.enabled = true;
        }

        DisableInformationText();
        informationTexts[0].enabled = true;

        // Disable buttons and sliders

        // Run the appropriate animations on buttons - should be it's own functions

    }

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