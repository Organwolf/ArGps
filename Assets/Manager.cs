using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLocation;

public class Manager : MonoBehaviour
{
    private CSV csv;
    private WaterMesh waterMesh;
    private DelaunayMesh delaunayMesh;
    private ARLocationProvider locationProvider;

    [SerializeField] double radius = 20.0;
    [SerializeField] Location deviceLocation;

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

        locationProvider = ARLocationProvider.Instance;
        Location x = locationProvider.LastLocation.ToLocation();
        Debug.Log($"Phones long: {x.Longitude} lat:{x.Latitude}");

        List<Location> csvWaterLocations = csv.PointsWithinRadius(deviceLocation, radius);
        waterMesh.SetPositionsToHandleLocations(csvWaterLocations);
        delaunayMesh.SetPositionsToHandleLocations(csvWaterLocations);
    }
}
