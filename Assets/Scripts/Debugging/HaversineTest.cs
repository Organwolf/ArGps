using ARLocation;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HaversineTest : MonoBehaviour
{
    public Location loc1;
    public Location loc2;

    private void Start()
    {
        var distance1 = HaversineDistance(loc1.Longitude, loc2.Longitude, loc1.Latitude, loc2.Latitude);
        var distance2 = HaversineDistance(loc2.Longitude, loc1.Longitude, loc2.Latitude, loc1.Latitude);
        //CSV_extended.ClosestPointGPS();

        Debug.Log("Dist1: " + distance1);
        Debug.Log("Dist2: " + distance2);
    }

    public static List<Location> ParseCsvFileUsingResources(string pathToFile)
    {
        var parsedData = new List<Location>();

        TextAsset file = Resources.Load(pathToFile) as TextAsset;
        string[] lines = file.text.Split('\n');

        Debug.Log("Rows in csv file: " + lines.Length);

        foreach(string line in lines)
        {
            string[] split = line.Split(',');

            double longitude = double.Parse(split[0]);
            double latitude = double.Parse(split[1]);
            bool building = (split[2] == "1");
            double height = double.Parse(split[3]);
            double waterHeight = double.Parse(split[4]) / 100;           // converting from cm to m
            double nearestNeighborHeight = double.Parse(split[5]);
            double nearestNeighborWater = double.Parse(split[6]) / 100f; // converting from cm to m

            var data = new Location
            {
                Longitude = longitude,
                Latitude = latitude,
                Building = building,
                Height = height,
                WaterHeight = waterHeight,
                NearestNeighborHeight = nearestNeighborHeight,
                NearestNeighborWater = nearestNeighborWater,
            };

            parsedData.Add(data);
        }

        return parsedData;
    }

    // These math functions should obviously be in their own class but for now they reside here.
    // Later on I will interpolate between ~4 points and return an average value. For now I find the closest.
    public static Location ClosestPoint(List<Location> locations, Location deviceLocation)
    {
        Location closestLocation = new Location();
        double minDistance = Double.MaxValue;
        double distanceToClosestPoint = -1;
        foreach (Location location in locations)
        {
            var distance = HaversineDistance(location.Longitude, deviceLocation.Longitude, location.Latitude, deviceLocation.Latitude);

            if (distance <= minDistance)
            {
                minDistance = distance;
                distanceToClosestPoint = distance;
                closestLocation = location;
            }
        }
        Debug.Log("Distance to closest point: " + distanceToClosestPoint);
        return closestLocation;
    }

    //public static List<Location> PointsWithinRadius(List<Location> locations, double radius, double longitude, double latitude)
    public static List<Location> PointsWithinRadius(List<Location> locations, double radius, Location deviceLocation)
    {
        //locationsWithinRadius.Clear();
        List<Location> locationsWithinRadius = new List<Location>();

        foreach (Location loc in locations)
        {
            var distance = HaversineDistance(loc.Longitude, deviceLocation.Longitude, loc.Latitude, deviceLocation.Latitude);

            if (distance <= radius)
                locationsWithinRadius.Add(loc);
        }
        return locationsWithinRadius;
    }

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
