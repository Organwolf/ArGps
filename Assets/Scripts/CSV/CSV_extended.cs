using ARLocation;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Thoughts: does this even need to be a monobehaviour?
 * 
 */

public class CSV_extended
{
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
            var data = new Location
            {
                Longitude = longitude,
                Latitude = latitude,
                Altitude = float.Parse(split[3]) + float.Parse(split[4]),
                Building = (split[2] == "1"), // Detta kan faktiskt funka men vi kan väl vänta med det?
                Height = float.Parse(split[3]), // Funkar height och waterHeight så kommer du få rätt på all den andra datan också. Ska kolla. Får lägga till utskrifter också för height är rätt stora värden
                WaterHeight = double.Parse(split[4]),
                NearestNeighborHeight = float.Parse(split[5]),
                NearestNeightborWater = float.Parse(split[6])
            };

            parsedData.Add(data);
        }

        return parsedData;
    }

    public static List<Location> PointsWithinRadius(List<Location> locations, double radius, double longitude, double latitude)
    {
        //locationsWithinRadius.Clear();
        List<Location> locationsWithinRadius = new List<Location>();

        foreach (Location loc in locations)
        {
            var distance = HaversineDistance(loc.Longitude, longitude, loc.Latitude, latitude);

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
