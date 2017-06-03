using Geolocation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elevation.Domain
{
    /// <summary>
    /// A single point within an elevation profile
    /// </summary>
    public class ElevationProfilePoint
    {
        public ElevationProfilePoint(DecimalGeoCoordinate coordinate, short elevation, double distance)
        {
            Coordinate = coordinate;
            ElevationInMetres = elevation;
            Distance = distance;
        }

        /// <summary>
        /// The coordinate of the point
        /// </summary>
        public DecimalGeoCoordinate Coordinate { get; private set; }
        /// <summary>
        /// The elevation at the specified co-ordinate in metres
        /// </summary>
        public short ElevationInMetres { get; private set; }
        /// <summary>
        /// The distance from the previous point in the profile.
        /// </summary>
        public double Distance { get; private set; }
    }
}
