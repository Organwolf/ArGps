using ARLocation;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class Manager : MonoBehaviour
    {
        [SerializeField] private string csvPath;
        [SerializeField] private ARLocationProvider locationProvider;
        [SerializeField] private GameObject spawnLocationPrefab;

        private TerrainVisualizer Visualizer => GetComponent<TerrainVisualizer>();

        private void Start()
        {
            if (Application.isEditor)
            {
                var locationReading = new LocationReading
                {
                    longitude = 14.20131381480994,
                    latitude = 55.70770763685751,
                };
                OnLocationProviderEnabled(locationReading);
            }
            else
            {
                locationProvider.OnEnabled.AddListener(OnLocationProviderEnabled);
                locationProvider.OnLocationUpdated.AddListener(locationReading => Debug.Log($"OnLocationUpdated: {locationReading.ToLocation()}"));
            }
        }

        private void OnLocationProviderEnabled(LocationReading reading)
        {
            Debug.Log($"OnLocationProviderEnabled {reading.ToString()}.");

            var placeAtOptions = new PlaceAtLocation.PlaceAtOptions();
            PlaceAtLocation.CreatePlacedInstance(spawnLocationPrefab, reading.ToLocation(), placeAtOptions);

            var csvData = CSVReader.Read(csvPath);
            csvData = CSVReader.MoveDataToPlayer(csvData, reading).ToList();

            //var current = new Coordinates(reading.longitude, reading.latitude);
            //Coordinates closestCoordinate = null;
            //var shortestDistance = double.MaxValue;
            //foreach (var data in csvData)
            //{
            //    var second = new Coordinates((double)data.Longitude, (double)data.Latitude);
            //    var distance = Coordinates.Distance(current, second);
            //    if (distance < shortestDistance)
            //    {
            //        shortestDistance = distance;
            //        closestCoordinate = second;
            //    }
            //}

            //Debug.Log($"Shortest distance is {(float)shortestDistance} meters.");
            //Debug.Log($"Current: {current.ToString()}    /    Closest: {closestCoordinate.ToString()}");

            var terrain = new Terrain();
            terrain.CreateTerrain(csvData, _ => OnTerrainCreated(_, reading));
        }

        private void OnTerrainCreated(Terrain terrain, LocationReading reading)
        {
            Debug.Log($"Terrain created with {terrain.Fragments.Count} fragments.");
            Visualizer.VisualizeTerrain(terrain, Coordinates.New(reading.longitude, reading.latitude));
        }
    }
}