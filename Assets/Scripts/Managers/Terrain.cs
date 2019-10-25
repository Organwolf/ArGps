using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class Terrain
    {
        public static Terrain CreateTerrainHelper(string pathToCsvFile, Action<Terrain> terrainCreated)
        {
            var terrain = new Terrain();
            terrain.CreateTerrain(pathToCsvFile, terrainCreated);
            return terrain;
        }

        public void CreateTerrain(string pathToCsvFile, Action<Terrain> terrainCreated)
        {
            Fragments = new List<TerrainFragment>();

            var file = Resources.Load(pathToCsvFile) as TextAsset;
            if (file == null) return;

            var lines = file.text.Split('\n');

            foreach (var line in lines)
            {
                var split = line.Split(',');

                var longitude = double.Parse(split[0]);
                var latitude = double.Parse(split[1]);
                //var building = (split[2] == "1");
                var height = double.Parse(split[3]);
                //var waterHeight = double.Parse(split[4]) / 100;           // converting from cm to m
                //var nearestNeighborHeight = double.Parse(split[5]);
                //var nearestNeighborWater = double.Parse(split[6]) / 100f; // converting from cm to m

                var coordinates = new Coordinates(longitude, latitude);
                var myFakeLocation = new Coordinates(13.188634, 55.687827);
                var data = new TerrainFragment(coordinates, height);

                Fragments.Add(data);
            }

            terrainCreated(this);
        }

        public List<TerrainFragment> Fragments { get; set; }

        public TerrainFragment GetFragment(Coordinates coordinates, double tolerance = 0.01f)
        {
            return GetFragment(coordinates.Longitude, coordinates.Latitude, tolerance);
        }
        public TerrainFragment GetFragment(double longitude, double latitude, double tolerance = 0.01f)
        {
            var fragment = Fragments.FirstOrDefault(terrainFragment =>
                Math.Abs(terrainFragment.Coordinates.Longitude - longitude) < tolerance &&
                Math.Abs(terrainFragment.Coordinates.Latitude - latitude) < tolerance);
            return fragment;
        }

        public IEnumerable<TerrainFragment> GetFragments(Coordinates centerPosition, double filterRadius)
        {
            return Filter(Fragments, centerPosition, filterRadius);
        }

        public static IEnumerable<TerrainFragment> Filter(IEnumerable<TerrainFragment> fragments, Coordinates centerPosition, double filterRadius)
        {
            return fragments.Where(fragment => fragment != null);
        }
    }
}