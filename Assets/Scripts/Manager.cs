using ARLocation;
using UnityEngine;

namespace Assets.Scripts
{
    public class Manager : MonoBehaviour
    {
        [SerializeField] private string csvPath;
        [SerializeField] private ARLocationProvider locationProvider;

        private TerrainVisualizer Visualizer => GetComponent<TerrainVisualizer>();

        private void Awake()
        {
            if (Application.isEditor)
            {
                var locationReading = new LocationReading
                {
                    longitude = 13.20131381480994,
                    latitude = 55.70770763685751,
                };
                OnLocationProviderEnabled(locationReading);
            }
            else
            {
                locationProvider.MockLocationData = null;
                locationProvider.OnEnabled.AddListener(OnLocationProviderEnabled);
            }
        }

        private void OnLocationProviderEnabled(LocationReading reading)
        {
            Debug.Log($"OnLocationProviderEnabled {reading.ToString()}.");
            var csvData = CSVReader.Read(csvPath);
            csvData = CSVReader.MoveDataToPlayer(csvData, reading);
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