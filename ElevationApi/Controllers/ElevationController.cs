using System;
using Microsoft.AspNetCore.Mvc;
using Geolocation;
using Elevation.Data.Files;
using System.IO;
using Microsoft.Extensions.Options;
using Elevation.Api.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Elevation.Domain;

namespace Elevation.Api.Controllers
{
    [Produces("application/json")]
    public class ElevationController : Controller
    {
        private readonly IElevationFileReader _elevationFileReader;
        private readonly String _elevationDataPath;
        private readonly IMemoryCache _cache;

        public ElevationController(IElevationFileReader elevationFileReader, IOptions<ElevationDataOptions> options, IMemoryCache cache)
        {
            if (options.Value.ElevationDataPath == null)
                throw new ArgumentException("No elevation data path specified in configuration file.");
            if (!Directory.Exists(options.Value.ElevationDataPath))
                throw new DirectoryNotFoundException($"Directory '{options.Value.ElevationDataPath}' does not exist.");

            _elevationDataPath = options.Value.ElevationDataPath;
            _elevationFileReader = elevationFileReader;
            _cache = cache;
        }

        /// <summary>
        /// Get the elevation readings for one or more coordinates.
        /// </summary>
        /// <param name="coordinates">A list of pipe separated decimal geo-coordinates, e.g. 54.4,-3.2|54.5,-3.5 </param>
        /// <returns></returns>
        [HttpGet("api/elevation/{coordinates}")]
        [ProducesResponseType(200, Type = typeof(ElevationPoint[]))]
        public IActionResult GetElevation(DecimalGeoCoordinate[] coordinates)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var requiredFiles = GetOrCreateRequiredFilesInCache(coordinates);

                var points = GetElevationPointsForCoordinates(requiredFiles, coordinates);

                return Ok(points);
            }
            catch(FileNotFoundException ex)
            {
                return StatusCode(501, new { response = $"Only datasets for the UK are currently available. {ex.Message}." });
            }
        }

        /// <summary>
        /// Get an elevation profile. Each result data point contains the cumulative distance from the starting point of the profile.
        /// </summary>
        /// <param name="coordinates">A list of pipe separated decimal geo-coordinates, e.g. 54.454225,-3.211586|54.5,-3.5 </param>
        /// <returns></returns>
        [HttpGet("api/elevation/profile/{coordinates}")]
        [ProducesResponseType(200, Type = typeof(ElevationProfilePoint[]))]
        public IActionResult GetProfile(DecimalGeoCoordinate[] coordinates)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var requiredFiles = GetOrCreateRequiredFilesInCache(coordinates);

                var profile = GetElevationProfileForCoordinates(requiredFiles, coordinates);

                return Ok(profile);
            }
            catch(FileNotFoundException ex)
            {
                return StatusCode(501, new { response = $"Only datasets for the UK are currently available. {ex.Message}." });
            }
        }           

        /// <summary>
        /// Get the list of elevation results for a list of coordinates
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="requiredFiles"></param>
        /// <returns></returns>
        private ElevationPoint[] GetElevationPointsForCoordinates(ElevationFile[] requiredFiles, DecimalGeoCoordinate[] coordinates)
        {
            List<ElevationPoint> results = new List<ElevationPoint>();

            foreach (var coordinate in coordinates)
            {
                var elevation = GetElevationAtCoordinate(requiredFiles, coordinate);
                results.Add(new ElevationPoint(coordinate, elevation));
            }

            return results.ToArray();
        }

        /// <summary>
        /// Get the elevation profile for a list of coordinates
        /// </summary>
        /// <param name="requiredFiles"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private ElevationProfilePoint[] GetElevationProfileForCoordinates(ElevationFile[] requiredFiles, DecimalGeoCoordinate[] coordinates)
        {
            if (coordinates == null || coordinates.Length == 0)
                return new ElevationProfilePoint[0];

            List<ElevationProfilePoint> results = new List<ElevationProfilePoint>();

            double distance = 0;
            DecimalGeoCoordinate previousPoint = coordinates[0];

            for (int index = 0; index < coordinates.Length; index++)
            {
                var coordinate = coordinates[index];

                var elevation = GetElevationAtCoordinate(requiredFiles, coordinate);

                distance += index == 0 
                    ? 0
                    : Math.Round(coordinate.DistanceTo(coordinates[index - 1]));

                results.Add(new ElevationProfilePoint(coordinate, elevation, distance));
            }

            return results.ToArray();
        }

        /// <summary>
        /// Retrieve the elevation for a coordinate from a set of elevation data sets
        /// </summary>
        /// <param name="elevationFiles"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        private short GetElevationAtCoordinate(ElevationFile[] elevationFiles, DecimalGeoCoordinate coordinate)
        {
            var fileContainingCoordinate = elevationFiles.FirstOrDefault(f => f.ContainsCoordinate(coordinate));

            return fileContainingCoordinate.GetElevationAtCoordinate(coordinate);
        }

        /// <summary>
        /// Retrieve the file names for list of required elevation data sets for the list of requested co-ordinates.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private String[] ParseFilenamesFromCoordinates(DecimalGeoCoordinate[] coordinates)
        {
            if (coordinates == null)
                return new String[0];

            return coordinates
                .Select(c => _elevationFileReader.ParseFileNameFromGeoCoordinate(c))
                .ToArray();
        }          

        /// <summary>
        /// Retrieve the required Elevation Files for the set of co-ordinates and add any new files to the cache
        /// </summary>
        /// <param name="coordinates"></param>
        private ElevationFile[] GetOrCreateRequiredFilesInCache(DecimalGeoCoordinate[] coordinates)
        {
            var filenames = ParseFilenamesFromCoordinates(coordinates);

            var requiredFiles = new List<ElevationFile>();

            foreach(var filename in filenames)
            {
                ElevationFile newFile = null;

                if (!_cache.TryGetValue(filename, out newFile))
                {
                    var filePath = Path.Combine(_elevationDataPath, filename);

                    if (!System.IO.File.Exists(filePath))
                        throw new FileNotFoundException($"Dataset '{filename}' is not available.");

                    newFile = _elevationFileReader.LoadFile(filePath);

                    // TODO : Add to config / update to a sensible option.
                    var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(1));

                    _cache.Set(filename, newFile, cacheOptions);
                }

                requiredFiles.Add(newFile);
            }

            return requiredFiles.ToArray();
        }
    }
}