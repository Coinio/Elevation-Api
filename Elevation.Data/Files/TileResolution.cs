using System;
using System.IO;

namespace Elevation.Domain.Files
{
    /// <summary>
    /// The arc second resolution for a tile set.
    /// This defines the number of readings taken by the SRTM satellites for every arc second of travel over the Earth's surface.
    /// </summary>
    public enum ResolutionDescription
    {
        None = 0,
        One = 1,
        Three = 3
    }

    /// <summary>
    /// The arc second resolution for a tile    
    /// </summary>
    public class TileResolution
    {
        private static readonly int _oneArcSecondResolutionRowLength = 3601;
        private static readonly int _threeArcSecondResolutionRowLength = 1201;

        /// <summary>
        /// The arc second resolution of the tile set.
        /// </summary>
        public ResolutionDescription Resolution { get; private set; }

        /// <summary>
        /// The length of a row for the tile set.
        /// This is defined by the tiles arc second resolution.
        /// </summary>
        public int RowLength { get; private set; }

        public static TileResolution GetResolutionFromFileLength(long length)
        {
            var resolution = GetResolutionDescriptionFromFileLength(length);

            return GetTileResolution(resolution);
        }

        public static TileResolution OneArcSecondTileResolution
        {
            get
            {
                return new TileResolution()
                {
                    Resolution = ResolutionDescription.One,
                    RowLength = _oneArcSecondResolutionRowLength
                };
            }
        }

        public static TileResolution ThreeArcSecondTileResolution
        {
            get
            {
                return new TileResolution()
                {
                    Resolution = ResolutionDescription.Three,
                    RowLength = _threeArcSecondResolutionRowLength
                };
            }
        }

        /// <summary>
        /// Get the appriopriate arc second resoluton description for the length of the file, i.e. Number of elevation data points contained with the file.
        /// The file size for the .hgt format is consistent for each file resolution
        /// </summary> 
        /// <param name="length"></param>
        /// <returns></returns>
        private static ResolutionDescription GetResolutionDescriptionFromFileLength(long length)
        {
            if (length == Math.Pow(_oneArcSecondResolutionRowLength, 2))
                return ResolutionDescription.One;
            if (length == Math.Pow(_threeArcSecondResolutionRowLength, 2))
                return ResolutionDescription.Three;

            throw new InvalidDataException("data.Count is not a valid size for a .hgt file.");
        }

        /// <summary>
        /// Get the appropriate tile resolution for the resolution description
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        private static TileResolution GetTileResolution(ResolutionDescription resolution)
        {
            // Set the row length from the resolution
            switch (resolution)
            {
                case ResolutionDescription.One:
                    return OneArcSecondTileResolution;
                case ResolutionDescription.Three:
                    return ThreeArcSecondTileResolution;
                default:
                    throw new NotImplementedException("No available row length.");
            }
        }
    }
}
