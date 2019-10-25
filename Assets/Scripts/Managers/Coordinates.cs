using System;

namespace Assets.Scripts.Managers
{
    public class Coordinates
    {
        public Coordinates(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public double Longitude { get; }
        public double Latitude { get; }
        public bool IsSame(Coordinates coordinate, double tolerance)
        {
            return IsSame(this, coordinate, tolerance);
        }

        public static bool IsSame(Coordinates firstCoordinate, Coordinates secondCoordinate, double tolerance)
        {
            return Math.Abs(firstCoordinate.Longitude - secondCoordinate.Longitude) < tolerance && Math.Abs(firstCoordinate.Latitude - secondCoordinate.Latitude) < tolerance;
        }

        public const double GenericTolerance = 0.00001f;

        public override bool Equals(object obj)
        {
            if (obj is Coordinates coordinate)
            {
                return IsSame(this, coordinate, GenericTolerance);
            }
            return false;
        }
        protected bool Equals(Coordinates other)
        {
            return Longitude.Equals(other.Longitude) && Latitude.Equals(other.Latitude);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Longitude.GetHashCode() * 397) ^ Latitude.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"({Longitude},{Latitude})";
        }

        public static Coordinates New(double longitude, double latitude) => new Coordinates(longitude, latitude);
    }
}