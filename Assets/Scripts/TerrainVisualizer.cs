using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ARLocation;
using UnityEngine;

namespace Assets.Scripts
{
    public class TerrainVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject locationObjectPrefab;
        [SerializeField] public PlaceAtLocation.PlaceAtOptions placementOptions = new PlaceAtLocation.PlaceAtOptions();
        [SerializeField] public bool debugMode;
        [SerializeField] public double filterRadius = 10;

        private List<Transform> placedObjects;

        private DelaunayMesh MeshGenerator => GetComponent<DelaunayMesh>();
        
        public void VisualizeTerrain(Terrain terrain, Coordinates centerPosition)
        {
            var terrainFragments = terrain.GetFragments(centerPosition, filterRadius).ToList();
            Debug.Log($"Starting to visualize terrain with {terrainFragments.Count} fragments.");
            StartCoroutine(PlaceLocationObjects(terrainFragments, OnPlacingComplete));
        }

        private IEnumerator PlaceLocationObjects(IEnumerable<TerrainFragment> terrainFragments, Action<List<Transform>> placingComplete)
        {
            yield return new WaitForSeconds(4);

            placedObjects = new List<Transform>();
            foreach (var locationData in terrainFragments)
            {
                var location = new Location(locationData.Latitude, locationData.Longitude, locationData.Altitude);
                var instance = PlaceAtLocation.CreatePlacedInstance(locationObjectPrefab, location, placementOptions, debugMode);
                var placeAtLocation = instance.GetComponent<PlaceAtLocation>();
                if (placeAtLocation.ObjectPositionUpdated == null) placeAtLocation.ObjectPositionUpdated = new PlaceAtLocation.ObjectUpdatedEvent();
                placeAtLocation.ObjectPositionUpdated.AddListener(OnObjectPositionUpdated);
                instance.name = location.ToString();
                placedObjects.Add(instance.transform);

                //yield return new WaitForSeconds(0.02f);
                //GenerateMeshFromTransforms(placedObjects);
                yield return new WaitForSeconds(0.02f);
            }

            //Wait for stabilizing
            var stabilizingWaitTime = 1.0f;
            yield return new WaitForSeconds(stabilizingWaitTime);
            placingComplete(placedObjects);
        }

        private void OnObjectPositionUpdated(GameObject arg0, Location arg1, int arg2)
        {
            GenerateMeshFromTransforms(placedObjects);
        }

        private void OnPlacingComplete(List<Transform> placedObjects)
        {
            //GenerateMeshFromTransforms(placedObjects);
        }

        private void GenerateMeshFromTransforms(IEnumerable<Transform> transforms)
        {
            var points = transforms.Select(trans => trans.position).ToList();
            if (points.Count > 3)
            {
                MeshGenerator.Generate(points, ARLocationManager.Instance.gameObject.transform, MeshGenerationCompleted);
            }
        }

        private void MeshGenerationCompleted()
        {
            //Debug.Log($"MeshGenerationCompleted.");
        }
    }
}
