using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    public class Terrain
    {
        public void CreateTerrain(IEnumerable<CsvRow> csvData, Action<Terrain> terrainCreated)
        {
            Fragments = new List<TerrainFragment>();
            
            foreach (var csvRow in csvData)
            {
                var coordinates = new Coordinates((double) csvRow.Longitude, (double) csvRow.Latitude);
                Fragments.Add(new TerrainFragment(coordinates, (double) csvRow.Altitude));
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
            return fragments.Where(fragment => fragment.Coordinates.DistanceTo(centerPosition) <= filterRadius);
        }
    }
}