using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLocation;

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
        // Event/Action experiment
        waterMesh.StringPrintAction += delaunayMesh.OnStringActionInvoked;

        //locationProvider = ARLocationProvider.Instance;
        //Location x = locationProvider.LastLocation.ToLocation();
        //Debug.Log($"Phones long: {x.Longitude} lat:{x.Latitude}");

        csvWaterLocation = csv.ReadAndParseCSV(pathToWaterCsv);
        var locationsWithinRadius = csv.PointsWithinRadius(deviceLocation, radius);
        waterMesh.SetPositionsToHandleLocations(locationsWithinRadius);
        delaunayMesh.SetPositionsToHandleLocations(locationsWithinRadius);
    }
}
