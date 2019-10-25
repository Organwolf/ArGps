using ARLocation;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class Manager : MonoBehaviour
    {
        [SerializeField] private string csvPath;
        [SerializeField] private ARLocationProvider locationProvider;

        private TerrainVisualizer Visualizer => GetComponent<TerrainVisualizer>();

        private void Start()
        {
            locationProvider.OnEnabled.AddListener(OnLocationProviderEnabled);
        }

        private void OnLocationProviderEnabled(LocationReading reading)
        {
            Terrain.CreateTerrainHelper(csvPath, terrain => OnTerrainCreated(terrain, reading));
        }

        private void OnTerrainCreated(Terrain terrain, LocationReading reading)
        {
            Visualizer.VisualizeTerrain(terrain, Coordinates.New(reading.longitude, reading.latitude));
        }
    }
}