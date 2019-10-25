namespace Assets.Scripts
{
    public class TerrainFragment
    {
        public TerrainFragment(Coordinates coordinates, double altitude)
        {
            Coordinates = coordinates;
            Altitude = altitude;
        }
        public Coordinates Coordinates { get; }
        public double Longitude => Coordinates.Longitude;

        public double Latitude => Coordinates.Latitude;
        public double Altitude { get; }
        public override string ToString()
        {
            return $"({Longitude},{Altitude},{Latitude})";
        }
    }
}