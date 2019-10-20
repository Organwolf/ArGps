using ARLocation;
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

    private CSV csv;
    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private WallPlacement wallPlacement;
    private ARLocationProvider locationProvider;
    private Transform groundPlaneTransform;

    private void Awake()
    {
        csv = GetComponent<CSV>();
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
        wallPlacement = GetComponent<WallPlacement>();
    }

    // Called each time the positions update
    private void OnPositionsUpdated(LocationsStateData stateData)
    {
        var locations = stateData.getLocalLocations();
        delaunayMesh.Generate(locations, groundPlaneTransform);
    }

    // UI
    public void AlterHeightOfMesh()
    {
        float meshHeight = delaunayMesh.GetHeightOfMesh();
        Debug.Log("Mesh height: " + meshHeight);
        var sliderValue = exaggerateHeightSlider.value;
        if(sliderValue > 0)
        {
            sliderValue *= 3;
            //delaunayMesh.SetHeightToMesh(meshHeight * sliderValue);
            delaunayMesh.SetHeightToMesh(sliderValue);
        }
    }

    public void GenerateWaterMesh()
    {
        // Send the ground plane transform to the delaunayMesh class
        groundPlaneTransform = wallPlacement.GetGroundPlaneTransform();

        waterMesh.enabled = true;
        var csvWaterLocation = csv.ReadAndParseCSV(pathToWaterCsv);
        var locationsWithinRadius = csv.PointsWithinRadius(deviceLocation, radius);
        waterMesh.SetPositionsToHandleLocations(locationsWithinRadius);

        waterMesh.PositionsUpdated += OnPositionsUpdated;
        delaunayMesh.SetPositionsToHandleLocations(locationsWithinRadius);
    }

    public void ResetSession()
    {
        waterMesh.enabled = false;
        wallPlacement.ResetSession();
    }
}