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
    //public List<AronsLocation> ListOfPositions
    //{
    //    get
    //    {
    //        return listOfPositions;
    //    }
    //}

    private void Start()
    {
        Debug.Log("Start triggered");

        locations = new List<Location>();
        locationsWithinRadius = new List<Location>();
        ReadAndParseCSV(pathToCsvFile);

        // UTM gis centrum
        var UTMEastingGis = 386915.91;
        var UTMNorthingGis = 6175124.26;

        // UTM well
        var UTMEastingWell = 386942.95;
        var UTMNorthingWell = 6175107.41;

        // Well
        var A = new Location(55.7085304, 13.2006627, 0);
        // GIS-center
        var B = new Location(55.708675, 13.200226, 0);

        var eucDist = Distance(UTMEastingGis, UTMNorthingGis, UTMEastingWell, UTMNorthingWell);
        Debug.Log("Distance euc: " + eucDist);
        var havDist = HaversineDistance(13.200226, 13.2006627, 55.708675, 55.7085304);
        Debug.Log("Distance hav: " + havDist);
    }

    // Weekend project 12-13/10
    // file from DTM: ar_dem_2m_84.csv
    // function that recives the location of the camera and returns the elevation 
    // interpolate around the phone 
    // GetHeight <-- recieve height of camera
    // foreach of the points from the CSV
    // h = p.h from DTM
    // w = p.w from the currente csv

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
            //Debug.Log("distance: " + distance);

            if (distance <= radius)
                locationsWithinRadius.Add(loc);
        }
        return locationsWithinRadius;
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

