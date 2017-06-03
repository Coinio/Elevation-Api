using System;
using System.Collections.Generic;
using System.IO;
using Geolocation;
using System.Text.RegularExpressions;
using System.Linq;
using Elevation.Domain;
using Elevation.Domain.Files;

namespace Elevation.Data.Files
{
    public class ElevationHgtFileReader : IElevationFileReader
    {
        /// <summary>
        /// Load a reference to an elevation file on disk.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public ElevationFile LoadFile(String filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            TileResolution resolution = null;

            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long fileLength = fileStream.Length / 2;
                resolution = TileResolution.GetResolutionFromFileLength(fileLength);
            }

            var location = ParseGeoCoordinateFromFileName(filePath);

            return new ElevationFile(filePath, location, resolution);
        }

        /// <summary>
        /// Creates a decimal geo-coordinate from a correctly formatted .hgt filename.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public DecimalGeoCoordinate ParseGeoCoordinateFromFileName(string fileName)
        {
            if (fileName == null)
                throw new NullReferenceException("fileName is null");

            // Remove file extension
            fileName = Path.GetFileName(fileName);
            fileName = fileName.Replace(".hgt", String.Empty);
            fileName = fileName.Trim();

            if (!IsValidHGTFileNameFormat(fileName))
                throw new ArgumentException("Invalid file name format, e.g. N52W002");

            double latitude = GetLatitudeFromFileName(fileName);
            double longitude = GetLongitudeFromFileName(fileName);

            return new DecimalGeoCoordinate(latitude, longitude);
        }

        /// <summary>
        /// Creates a correctly formated .hgt filename from a geo-coordinate
        /// </summary>
        /// <param name="geoCoordinate"></param>
        /// <returns></returns>
        public string ParseFileNameFromGeoCoordinate(DecimalGeoCoordinate geoCoordinate)
        {
            bool isSouth = geoCoordinate.Latitude <= 0;
            bool isWest = geoCoordinate.Longitude < 0;

            double roundedLatitude = Math.Floor(geoCoordinate.Latitude);
            double roundedLongitude = Math.Floor(geoCoordinate.Longitude);

            String latitudeString = GetLatitudeAsString(roundedLatitude, isSouth);
            String longitudeString = GetLongitudeAsString(roundedLongitude, isWest);

            return $"{latitudeString}{longitudeString}.hgt";
        }

        /// <summary>
        /// Check whether a .hgt file name is valid
        /// The file name should be of the format N52W002, i.e Valid geo-coordinates.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool IsValidHGTFileNameFormat(String fileName)
        {
            String fullPattern = "^[NS][0-9]{2}[WE][0-9]{3}$";

            // Make sure we have a valid file name format
            Regex regex = new Regex(fullPattern);

            if (!regex.IsMatch(fileName))
                return false;

            return true;
        }

        private string GetLongitudeAsString(double longitude, bool isWest)
        {
            String longitudeString = longitude.ToString();

            longitudeString = isWest
                ? longitudeString.Replace("-", "").PadLeft(3, '0').Insert(0, "W")
                : longitudeString.PadLeft(3, '0').Insert(0, "E");

            return longitudeString;
        }

        private string GetLatitudeAsString(double latitude, bool isSouth)
        {
            String latitudeString = latitude.ToString();

            latitudeString = isSouth
                ? latitudeString.Replace("-", "").PadLeft(2, '0').Insert(0, "S")
                : latitudeString.PadLeft(2, '0').Insert(0, "N");

            return latitudeString;
        }

        private double GetLatitudeFromFileName(String fileName)
        {
            String latitudePattern = "^[NS][0-9]{2}";
            double latitudeValue = 0;

            // Parse out the latitude
            Regex regex = new Regex(latitudePattern);

            if (!regex.IsMatch(fileName))
                throw new ArgumentException("Invalid latitude in file name");

            String latitudeString = regex.Match(fileName).Value;

            latitudeString = latitudeString.Replace('N', ' ');
            latitudeString = latitudeString.Replace('S', '-');
            latitudeString = latitudeString.Trim();

            if (!Double.TryParse(latitudeString, out latitudeValue))
                throw new FormatException("Unable to parse latitude from latitudeString");

            return latitudeValue;
        }

        private double GetLongitudeFromFileName(String fileName)
        {
            String longitudePattern = "[WE][0-9]{3}$";
            double longitudeValue = 0;

            Regex regex = new Regex(longitudePattern);

            if (!regex.IsMatch(fileName))
                throw new ArgumentException("Invalid longitude in file name");

            String longitudeString = regex.Match(fileName).Value;

            longitudeString = longitudeString.Replace('W', '-');
            longitudeString = longitudeString.Replace('E', ' ');
            longitudeString = longitudeString.Trim();

            if (!Double.TryParse(longitudeString, out longitudeValue))
                throw new FormatException("Unable to parse longitude from longitudeString");

            return longitudeValue;
        }
        /// <summary>
        /// Load the elevation data from the specified file.
        /// The .hgt is essentially just a flat file containing a grid of elevation points for region / tile which the file covers.
        /// We are just reading through the file two-bytes at a time (as each point is stored as a short) and converting to little-endian.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private short[] LoadElevationDataFromFile(String filePath)
        {
            var data = new List<short>();
                     
            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new BinaryReader(fileStream))
            {
                int position = 0;
                long streamLength = reader.BaseStream.Length;
                
                while (position < streamLength)
                {
                    byte[] byteData = new byte[2];
                    reader.Read(byteData, 0, 2);

                    Int16 convertedData = ConvertToShort(byteData);

                    data.Add(convertedData);

                    position += sizeof(short);
                }
            }

            return data.ToArray();
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
