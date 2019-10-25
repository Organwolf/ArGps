using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ARLocation;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class TerrainVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject locationObjectPrefab;
        [SerializeField] public PlaceAtLocation.PlaceAtOptions placementOptions = new PlaceAtLocation.PlaceAtOptions();
        [SerializeField] public bool debugMode;
        [SerializeField] public double filterRadius = 10;

        //private List<Transform> placedObjects;

        private DelaunayMesh MeshGenerator => GetComponent<DelaunayMesh>();
        
        public void VisualizeTerrain(Terrain terrain, Coordinates centerPosition)
        {
            var terrainFragments = terrain.GetFragments(centerPosition, filterRadius);
            StartCoroutine(PlaceLocationObjects(terrainFragments, OnPlacingComplete));
        }

        private IEnumerator PlaceLocationObjects(IEnumerable<TerrainFragment> terrainFragments, Action<List<Transform>> placingComplete)
        {
            var placedObjects = new List<Transform>();
            foreach (var locationData in terrainFragments)
            {
                var location = new Location(locationData.Latitude, locationData.Longitude, locationData.Altitude);
                var instance = PlaceAtLocation.CreatePlacedInstance(locationObjectPrefab, location, placementOptions, debugMode);
                placedObjects.Add(instance.transform);
                yield return new WaitForSeconds(0.1f);
            }

            placingComplete(placedObjects);
        }

        private void OnPlacingComplete(List<Transform> placedObjects)
        {
            var points = placedObjects.Select(trans => trans.position);
            MeshGenerator.Generate(points, transform);
        }
    }
}