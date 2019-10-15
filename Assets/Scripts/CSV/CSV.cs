using ARLocation;
using ARLocation.Utils;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CSV : MonoBehaviour
{
    [SerializeField] string pathToCsvFile;

    private List<Location> locations;
    private List<Location> locationsWithinRadius;

    private void Start()
    {
        locations = new List<Location>();
        locationsWithinRadius = new List<Location>();
        ReadAndParseCSV(pathToCsvFile);
    }

    public void ReadAndParseCSV(string pathToCsvFile)
    {
        TextAsset entireCSV = Resources.Load(pathToCsvFile) as TextAsset;
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

            locations.Add(new Location(latitude, longitude, altitude));
        }
    }

    public List<Location> PointsWithinRadius(Location phone, double radius)
    {
        locationsWithinRadius.Clear();

        foreach(Location loc in locations)
        {
            var distance = HaversineDistance(loc.Longitude, phone.Longitude, loc.Latitude, phone.Latitude);

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

