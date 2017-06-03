using Geolocation;
using System;

namespace Elevation.Data.Files
{
    public interface IElevationFileReader
    {
        /// <summary>
        /// Load a reference to an elevation file on disk.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        ElevationFile LoadFile(String filePath);

        /// <summary>
        /// Creates a decimal geo-coordinate from a correctly formatted .hgt filename.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        DecimalGeoCoordinate ParseGeoCoordinateFromFileName(String fileName);

        /// <summary>
        /// Creates a correctly formatted .hgt filename from a geo-coordinate
        /// </summary>
        /// <param name="geoCoordinate"></param>
        /// <returns></returns>
        String ParseFileNameFromGeoCoordinate(DecimalGeoCoordinate geoCoordinate);

        /// <summary>
        /// Check whether a .hgt file name is valid
        /// The file name should be of the format N52W002, i.e Valid geo-coordinates.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool IsValidHGTFileNameFormat(String fileName);
    }
}
