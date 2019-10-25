using ARLocation;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocationData
{
    public double Longitude;
    public double Latitude;
    public double Altitude;

    public bool Building;
    public double Height;
    public double WaterHeight;
    public double NearestNeighborWater;
    public double NearestNeighborHeight;
}

public class CSV_extended
{
    public static List<LocationData> ParseCsvFileUsingResources(string pathToFile)
    {
        var parsedData = new List<LocationData>();

        var file = Resources.Load(pathToFile) as TextAsset;
        if (file == null) return parsedData;

        var lines = file.text.Split('\n');

        foreach (var line in lines)
        {
            var split = line.Split(',');

            var longitude = double.Parse(split[0]);
            var latitude = double.Parse(split[1]);
            var building = (split[2] == "1");
            var height = double.Parse(split[3]);
            var waterHeight = double.Parse(split[4]) / 100;           // converting from cm to m
            var nearestNeighborHeight = double.Parse(split[5]);
            var nearestNeighborWater = double.Parse(split[6]) / 100f; // converting from cm to m

            var data = new LocationData
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

    public static LocationData ClosestPoint(List<LocationData> locations, LocationData deviceLocation)
    {
        var closestLocation = new LocationData();
        var minDistance = Double.MaxValue;
        double distanceToClosestPoint = -1;
        foreach (var location in locations)
        {
            var distance = HaversineDistance(location.Longitude, deviceLocation.Longitude, location.Latitude, deviceLocation.Latitude);

            if (distance <= minDistance)
                minDistance = distance;
            distanceToClosestPoint = distance;
            closestLocation = location;
        }
        Debug.Log("Distance to closest point: " + distanceToClosestPoint);
        return closestLocation;
    }

    //public static List<Location> PointsWithinRadius(List<Location> locations, double radius, double longitude, double latitude)
    public static List<LocationData> PointsWithinRadius(List<LocationData> locations, double radius, Location deviceLocation)
    {
        //locationsWithinRadius.Clear();
        var locationsWithinRadius = new List<LocationData>();

        foreach (var loc in locations)
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
