using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLocation;
using static ObjectLocationManager;

public class Manager : MonoBehaviour
{
    public double radius = 20.0;
    public Location deviceLocation;

    CSV csv;
    DelaunayMesh dMesh;
    ObjectLocationManager objLocManager;

    private void Awake()
    {
        csv = GetComponent<CSV>();
        dMesh = GetComponent<DelaunayMesh>();
        objLocManager = GetComponent<ObjectLocationManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        var csvLocation = csv.PointsWithinRadius(deviceLocation, radius);
        objLocManager.SetPositionsToHandleLocations(csvLocation);

        objLocManager.LocationsStateDataChange += dMesh.OnLocationsStateDataChange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
