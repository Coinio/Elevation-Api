using Elevation.Domain;
using Elevation.Domain.Files;
using Geolocation;
using System;
using System.IO;
using System.Linq;


namespace Elevation.Data.Files
{
    /// <summary>
    /// This represents a 'reference' to an existing elevation data file on disk.
    /// The reason for this class is to contain the data required to run elevation queries across multiple files without the initial overhead of loading them into memory first.
    /// It provides a pre-calculated location and tile resolution so this doesn't have to be done multiple times.
    /// </summary>
    public class ElevationFile
    {
        /// <summary>
        /// The path of the file
        /// </summary>
        public String FilePath { get; private set; }
        /// <summary>
        /// This represents the location of the files bottom left corner
        /// </summary>
        public DecimalGeoCoordinate Location { get; private set; }
        /// <summary>
        /// The arc second resolution of the data in file.
        /// </summary>
        public TileResolution Resolution { get; private set; }

        public ElevationFile(String filePath, DecimalGeoCoordinate location, TileResolution resolution)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            Resolution = resolution;
            Location = location;
            FilePath = filePath;
        }

        /// <summary>
        /// Does the tile contain the specified co-ordinate?
        /// </summary>
        /// <param name="targetLocation"></param>
        /// <returns></returns>
        public bool ContainsCoordinate(DecimalGeoCoordinate targetLocation)
        {
            if (targetLocation == null)
                throw new ArgumentNullException("targetLocation is null");

            if (targetLocation.Latitude < Location.Latitude)
                return false;
            if (targetLocation.Latitude > Location.Latitude + 1)
                return false;
            if (targetLocation.Longitude < Location.Longitude)
                return false;
            if (targetLocation.Longitude > Location.Longitude + 1)
                return false;

            return true;
        }

        /// <summary>
        /// Get the elevation reading at the specified coordinate
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public short GetElevationAtCoordinate(DecimalGeoCoordinate coordinate)
        {
            int index = GetIndexAtCoordinate(coordinate);

            using (var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek(index * 2, SeekOrigin.Begin);

                byte[] byteData = new byte[2];
                fileStream.Read(byteData, 0, 2);

                short elevation = ConvertToShort(byteData);

                return elevation;
            }
        }

        /// <summary>
        /// Get the relative file index for the specified coordinate within the tile
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public int GetIndexAtCoordinate(DecimalGeoCoordinate coordinate)
        {
            if (coordinate == null)
                throw new NullReferenceException("location is null");
            if (!ContainsCoordinate(coordinate))
                throw new ArgumentOutOfRangeException($"'{coordinate.ToString()}' is not contained within specified file '{FilePath}'.");

            double relativeLatitude = (coordinate.Latitude - Location.Latitude);
            double relativeLongitude = (coordinate.Longitude - Location.Longitude);

            int latitudeRow = (int)Math.Round(relativeLatitude * (Resolution.RowLength - 1));
            int longitudeColumn = (int)Math.Round(relativeLongitude * (Resolution.RowLength - 1));

            latitudeRow = InvertIndex(latitudeRow, Resolution.RowLength - 1);

            return ConvertXYtoIndex(longitudeColumn, latitudeRow, Resolution.RowLength);
        }

        /// <summary>
        /// Invert the index based on the row length
        /// </summary>
        /// <param name="index"></param>
        /// <param name="rowLength"></param>
        /// <returns></returns>
        private int InvertIndex(int index, int rowLength)
        {
            return rowLength - index;
        }

        /// <summary>
        /// Convert an xy co-ordinate to an array index based on rowlength
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rowLength"></param>
        /// <returns></returns>
        private int ConvertXYtoIndex(int x, int y, int rowLength)
        {
            return x + (y * rowLength);
        }

        /// <summary>
        /// Convert the bytes to little endian and return the value
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private short ConvertToShort(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);
            else
                return BitConverter.ToInt16(bytes, 0);
        }
    }
}
