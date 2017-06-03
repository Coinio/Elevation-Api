using System;
using Geolocation;

namespace Elevation.Domain
{
    public struct ElevationPoint : IEquatable<ElevationPoint>
    {
        public ElevationPoint(DecimalGeoCoordinate coordinates, short elevationInMetres)
            : this()
        {
            Coordinates = coordinates;
            ElevationInMetres = elevationInMetres;
        }
       
        /// <summary>
        /// The co-ordinates of the elevation data point
        /// </summary>
        public DecimalGeoCoordinate Coordinates { get; private set; }
        /// <summary>
        /// The elevation of the data point in metres
        /// </summary>
        public short ElevationInMetres { get; private set; }
        
        public override string ToString()
        {
            return String.Format("{0} Elevation: {1}m", Coordinates, ElevationInMetres);
        }

        #region Equality overrides

        public bool Equals(ElevationPoint other)
        {
            if (other == null)
                return false;

            if (this.Coordinates != other.Coordinates)
                return false;
            if (this.ElevationInMetres != other.ElevationInMetres)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is ElevationPoint))
                return false;

            ElevationPoint coord = (ElevationPoint) obj;

            if (coord == null)
                return false;

            return Equals(coord);
        }

        public static bool operator ==(ElevationPoint coord1, ElevationPoint coord2)
        {
            return coord1.Equals(coord2);
        }

        public static bool operator !=(ElevationPoint coord1, ElevationPoint coord2)
        {
            return !coord1.Equals(coord2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;

                hash = (hash * 23) + this.Coordinates.GetHashCode();
                hash = (hash * 23) + this.ElevationInMetres;

                return hash;
            }
        }


        #endregion
    }
}
