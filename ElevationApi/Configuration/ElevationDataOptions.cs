using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elevation.Api.Configuration
{
    /// <summary>
    /// The configuration options for the elevation data
    /// </summary>
    public class ElevationDataOptions
    {
        /// <summary>
        /// The path to the folder which contains the elevation data
        /// </summary>
        public String ElevationDataPath { get; set; }
    }
}
