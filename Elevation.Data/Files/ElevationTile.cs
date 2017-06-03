using Elevation.Data.Files;
using Geolocation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elevation.Domain
{
    /// <summary>
    /// A class to hold the data for an elevation tile in memory
    /// </summary>
    public class ElevationTile
    {
        /// <summary>
        /// The raw tile data from the file
        /// </summary>
        private readonly short[] _data;

        public ElevationTile(ElevationFile fileReference, short[] data)
        {
            ElevationFile = fileReference ?? throw new ArgumentNullException("fileReference");

            _data = data ?? throw new ArgumentNullException("rawData");
        }

        /// <summary>
        /// The reference to the elevation file 
        /// </summary>
        public ElevationFile ElevationFile { get; private set; } 
        
        /// <summary>
        /// Get the raw data for the elevation tile
        /// </summary>
        public int[] Data
        {
            get
            {
                var copy = new int[_data.Length];
                _data.CopyTo(copy, 0);

                return copy;
            }
        }
        
        /// <summary>
        /// Retrieve the elevation for the specified coordinate
        /// </summary>
        /// <param name="dataSetLocation"></param>
        /// <returns></returns>
        public int GetElevationAtCoordinate(DecimalGeoCoordinate coordinate)
        {
            if (coordinate == null)
                throw new NullReferenceException("location is null");

            var index = ElevationFile.GetIndexAtCoordinate(coordinate);

            return _data[index];
        }
    }
}
