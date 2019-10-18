﻿using ARLocation;
using UnityEngine;
using static WaterMesh;

public class Manager : MonoBehaviour
{
    [SerializeField] string pathToWaterCsv;
    [SerializeField] double radius = 20.0;
    [SerializeField] Location deviceLocation;
    [SerializeField] PlacementManager placementManager;

    private CSV csv;
    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private ARLocationProvider locationProvider;
    private Transform groundPlaneTransform;


    private void Awake()
    {
        csv = GetComponent<CSV>();
        waterMesh = GetComponent<WaterMesh>();
        delaunayMesh = GetComponent<DelaunayMesh>();
    }

    public void GenerateWaterMesh()
    {
        // Send the ground plane transform to the delaunayMesh class
        groundPlaneTransform = placementManager.GetGroundPlaneTransform();

        waterMesh.enabled = true;
        var csvWaterLocation = csv.ReadAndParseCSV(pathToWaterCsv);
        var locationsWithinRadius = csv.PointsWithinRadius(deviceLocation, radius);
        waterMesh.SetPositionsToHandleLocations(locationsWithinRadius);

        waterMesh.PositionsUpdated += OnPositionsUpdated;
        delaunayMesh.SetPositionsToHandleLocations(locationsWithinRadius);
    }

    void Start()
    {
        // write a function that returns the devices current position from WaterMesh - Rename WaterMesh

    }

    private void OnPositionsUpdated(LocationsStateData stateData)
    {
        // Denna kommer anropas varje gång en eller flera positoner blir uppdaterade.
        // Så nu kommer du tom få tillgång till alla positioner som blivit uppdaterade.
        var locations = stateData.getLocalLocations();
        delaunayMesh.Generate(locations, groundPlaneTransform);
    }
}