using System;

namespace Geolocation
{
    /// <summary>
    /// A class to hold longitude and latitude in decimal degree format
    /// </summary>
    public struct DecimalGeoCoordinate : IEquatable<DecimalGeoCoordinate>
    {

        public DecimalGeoCoordinate(double latitude, double longitude)
            : this()
        {
            AssignLongitude(longitude);
            AssignLatitude(latitude);
        }

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        /// <summary>
        /// Is this a valid longitude value?
        /// </summary>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static bool IsValidLongitude(double longitude)
        {
            if (longitude < -180 || longitude > 180)
                return false;

            return true;
        }

        /// <summary>
        /// Is this a valid latitude value?
        /// </summary>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public static bool IsValidLatitude(double latitude)
        {
            if (latitude < -90 || latitude > 90)
                return false;

            return true;
        }

        /// <summary>
        /// Attempt to parse a valid DecimalGeoCoordinate from a latitude / longitude input
        /// </summary>
        /// <param name="latitudeString"></param>
        /// <param name="longitudeString"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static bool TryParse(String latitudeString, String longitudeString, out DecimalGeoCoordinate coordinate)
        {
            double latitude = 0, longitude = 0;
            coordinate = new DecimalGeoCoordinate(0, 0);

            if (!double.TryParse(latitudeString, out latitude))
                return false;
            if (!IsValidLatitude(latitude))
                return false;
            if (!double.TryParse(longitudeString, out longitude))
                return false;
            if (!IsValidLongitude(longitude))
                return false;

            coordinate = new DecimalGeoCoordinate(latitude, longitude);

            return true;
        }

        /// <summary>
        /// Gets the distance between markers in metres. Haversine formula. 
        /// Thanks to http://damien.dennehy.me/blog/2011/01/15/haversine-algorithm-in-csharp/.
        /// </summary>
        /// <param name="otherCoordinate"></param>
        /// <returns></returns>
        public double DistanceTo(DecimalGeoCoordinate otherCoordinate)
        {
            if (otherCoordinate == null)
                throw new ArgumentNullException("otherCoordinate is null");

            double earthRadiusInMetres = 6371009;
            double dLat = DegreesToRadians(otherCoordinate.Latitude - Latitude);
            double dLon = DegreesToRadians(otherCoordinate.Longitude - Longitude);

            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(DegreesToRadians(Latitude)) * Math.Cos(DegreesToRadians(otherCoordinate.Latitude)) *
                    Math.Pow(Math.Sin(dLon / 2), 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = earthRadiusInMetres * c;

            return distance;
        }

        public bool Equals(DecimalGeoCoordinate other)
        {
            if (other == null)
                return false;

            double differenceTolerance = 0.00000001;
            double latitudeDifference = Math.Abs(this.Latitude * differenceTolerance);
            double longitudeDifference = Math.Abs(this.Longitude * differenceTolerance);

            if (Math.Abs(this.Latitude - other.Latitude) > latitudeDifference)//this.Latitude != other.Latitude
                return false;
            if (Math.Abs(this.Longitude - other.Longitude) > longitudeDifference)//this.Longitude != other.Longitude
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            DecimalGeoCoordinate coord = (DecimalGeoCoordinate)obj;

            if (coord == null)
                return false;

            return Equals(coord);
        }

        public override string ToString()
        {
            return String.Format("Latitude: {0}, Longitude: {1}", Latitude, Longitude);
        }

        public static bool operator ==(DecimalGeoCoordinate coord1, DecimalGeoCoordinate coord2)
        {
            return coord1.Equals(coord2);
        }

        public static bool operator !=(DecimalGeoCoordinate coord1, DecimalGeoCoordinate coord2)
        {
            return !coord1.Equals(coord2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;

                hash = (hash * 23) + Latitude.GetHashCode();
                hash = (hash * 23) + Longitude.GetHashCode();

                return hash;
            }
        }

        private void AssignLongitude(double longitude)
        {
            if (!IsValidLongitude(longitude))
                throw new ArgumentOutOfRangeException("Invalid longitude");

            Longitude = Math.Round(longitude, 8);
        }

        private void AssignLatitude(double latitude)
        {
            if (!IsValidLatitude(latitude))
                throw new ArgumentOutOfRangeException("Invalid latitude");

            Latitude = Math.Round(latitude, 8);
        }

        private double DegreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        private double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }
    }
}
