using System;
using System.Collections.Generic;
using ARLocation;
using UnityEngine;

namespace Assets.Scripts
{
    public class CsvRow
    {
        public decimal Longitude;
        public decimal Latitude;
        public decimal Altitude;

        public CsvRow(decimal longitude, decimal latitude, decimal altitude)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
        }
    }

    public class CSVReader
    {
        public static IEnumerable<CsvRow> Read(string pathToCsvFile)
        {
            var random = new System.Random();
            var toReturn = new List<CsvRow>();
            var file = Resources.Load(pathToCsvFile) as TextAsset;
            if (file == null) return toReturn;

            var lines = file.text.Split('\n');

            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                var split = line.Split(',');

                var longitudeString = split[0].Replace('.', ',');
                if (!decimal.TryParse(longitudeString, out var longitude))
                    throw new Exception($"Was not able to parse{split[0]}.");
                if (longitude == 0)
                    throw new Exception($"Was not able to parse{split[0]}.");

                var latitudeString = split[1].Replace('.', ',');
                if (!decimal.TryParse(latitudeString, out var latitude))
                    throw new Exception($"Was not able to parse{split[1]}.");
                if (latitude == 0)
                    throw new Exception($"Was not able to parse{split[0]}.");

                const double minHeight = 0;
                const double maxHeight = 1;
                var height = random.NextDouble() * (maxHeight - minHeight);

                toReturn.Add(new CsvRow(longitude, latitude, (decimal) height));
            }

            return toReturn;
        }

        public static IEnumerable<CsvRow> MoveDataToPlayer(IEnumerable<CsvRow> csvData, LocationReading reading)
        {
            var toReturn = new List<CsvRow>();
            toReturn.AddRange(csvData);

            var minLong = decimal.MaxValue;
            var minLat = decimal.MaxValue;
            var maxLong = decimal.MinValue;
            var maxLat = decimal.MinValue;

            foreach (var csvRow in toReturn)
            {
                if (csvRow.Longitude > maxLong) maxLong = csvRow.Longitude;
                if (csvRow.Longitude < minLong) minLong = csvRow.Longitude;
                if (csvRow.Latitude > maxLat) maxLat = csvRow.Latitude;
                if (csvRow.Latitude < minLat) minLat = csvRow.Latitude;
            }

            decimal i = 2;
            foreach (var csvRow in toReturn)
            {
                csvRow.Longitude -= minLong;
                csvRow.Longitude += (decimal)reading.longitude;
                var l = (maxLong - minLong);
                var csvRowLongitude = (l / i);
                csvRow.Longitude -= csvRowLongitude;

                csvRow.Latitude -= minLat;
                csvRow.Latitude += (decimal)reading.latitude;
                csvRow.Latitude -= ((maxLat - minLat) / i);
            }

            return toReturn;
        }
    }
}