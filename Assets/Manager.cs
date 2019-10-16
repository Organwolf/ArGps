using System.Collections;
using ARLocation;
using System.Collections.Generic;
using UnityEngine;
using static WaterMesh;

public class Manager : MonoBehaviour
{
    [SerializeField] string pathToWaterCsv;
    [SerializeField] double radius = 20.0;
    [SerializeField] Location deviceLocation;

    private CSV csv;
    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private ARLocationProvider locationProvider;
    private List<Location> csvWaterLocation;

    private void Awake()
    {
        csv = GetComponent<CSV>();
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
    }

    void Start()
    {
        // write a function that returns the devices current position from WaterMesh - Rename WaterMesh

        csvWaterLocation = csv.ReadAndParseCSV(pathToWaterCsv);
        var locationsWithinRadius = csv.PointsWithinRadius(deviceLocation, radius);
        waterMesh.SetPositionsToHandleLocations(locationsWithinRadius);

        waterMesh.PositionsUpdated += OnPositionsUpdated;
        delaunayMesh.SetPositionsToHandleLocations(locationsWithinRadius);
    }

    private void OnPositionsUpdated(LocationsStateData stateData)
    {
        // Denna kommer anropas varje gång en eller flera positoner blir uppdaterade.
        // Så nu kommer du tom få tillgång till alla positioner som blivit uppdaterade.
        var locations = stateData.getLocalLocations();
        delaunayMesh.Generate(locations);
    }
}