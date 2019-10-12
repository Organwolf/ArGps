using ARLocation;
using ARLocation.Utils;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CSV : MonoBehaviour
{
    [SerializeField] string pathToCsvWaterFile;
    [SerializeField] string pathToCsvHeightFile;

    private List<Location> waterHeightLocations;
    private List<Location> locationsWithinRadius;
    private List<Location> heightAtLocations;

    private void Start()
    {
        Debug.Log("Start triggered");

        waterHeightLocations = new List<Location>();
        locationsWithinRadius = new List<Location>();
        heightAtLocations = new List<Location>();
        ReadAndParseCSV(pathToCsvWaterFile);
        ReadAndParseCSVHieght(pathToCsvHeightFile);
    }


    // Weekend project 12-13/10
    // file from DTM: ar_dem_2m_84.csv
    // function that recives the location of the camera and returns the elevation 
    // interpolate around the phone 
    // GetHeight <-- recieve height of camera
    // foreach of the points from the CSV
    // h = p.h from DTM
    // w = p.w from the currente csv

    private void ReadAndParseCSVHieght(string pathToCsvHeightFile)
    {
        TextAsset entireCSV = Resources.Load(pathToCsvHeightFile) as TextAsset;
        var lines = entireCSV.text.Split('\n');

        foreach (var line in lines)
        {
            var locationString = line.Split(',');

            var longitudeString = locationString[0].Trim();
            float longitude;

            if (float.TryParse(longitudeString, out longitude) == false)
            {
                longitude = 0;
            }

            var latitudeString = locationString[1].Trim();
            float latitude;

            if (float.TryParse(latitudeString, out latitude) == false)
            {
                latitude = 0;
            }

            var heightString = locationString[2].Trim();
            float altitude;
            if (float.TryParse(heightString, out altitude) == false)
            {
                altitude = 0;
            }

            heightAtLocations.Add(new Location(latitude, longitude, altitude));
        }
    }

    public void ReadAndParseCSV(string pathToCsvWaterFile)
    {
        TextAsset entireCSV = Resources.Load(pathToCsvWaterFile) as TextAsset;
        var lines = entireCSV.text.Split('\n');

        foreach (var line in lines)
        {
            var locationString = line.Split(',');

            var longitudeString = locationString[0].Trim();
            float longitude;

            if (float.TryParse(longitudeString, out longitude) == false)
            {
                longitude = 0;
            }

            var latitudeString = locationString[1].Trim();
            float latitude;

            if (float.TryParse(latitudeString, out latitude) == false)
            {
                latitude = 0;
            }

            var altitudeString = locationString[2].Trim();
            float altitude;
            if (float.TryParse(altitudeString, out altitude) == false)
            {
                altitude = 0;
            }

            waterHeightLocations.Add(new Location(latitude, longitude, altitude));
        }
    }


    // Water height csv should actually be fed into this algorithm
    public List<Location> PointsWithinRadius(Location deviceLocation, double radius)
    {
        locationsWithinRadius.Clear();

        foreach(Location loc in waterHeightLocations)
        {
            var distance = HaversineDistance(loc.Longitude, deviceLocation.Longitude, loc.Latitude, deviceLocation.Latitude);
            //Debug.Log("distance: " + distance);

            if (distance <= radius)
                locationsWithinRadius.Add(loc);
        }
        return locationsWithinRadius;
    }

    public float GetHeight(Location deviceLocation)
    {
        float closestHeightValue = float.MaxValue;

        foreach (Location loc in heightAtLocations)
        {
            var currentDistance = (float)HaversineDistance(loc.Longitude, deviceLocation.Longitude, loc.Latitude, deviceLocation.Latitude);

            if (currentDistance < closestHeightValue)
            {
                closestHeightValue = currentDistance;
            }
            
        }
        return closestHeightValue;
    }

    private double Distance(double x1, double y1, double x2, double y2) => Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

    public static double HaversineDistance(double long1, double long2, double lat1, double lat2)
    {
        var R = 6371000; // earths diameter metres
        var deltaLat = DegreeToRadian(lat2 - lat1);
        var deltaLong = DegreeToRadian(long2 - long1);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLong / 2) * Math.Sin(deltaLong / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = R * c;

        return distance;
    }

    private static double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}

