using Elevation.Data.Files;
using Geolocation;
using System;
using System.Diagnostics;
using System.Threading;

namespace Elevation.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var elevationTile1 = new ElevationHgtFileReader().LoadTile(@"D:\Code\Projects\Elevation\TestData\STRM1\N54W004.hgt");

            var elevationFileReader = new ElevationHgtFileReader();

            var fileRef = elevationFileReader.LoadFile(@"D:\Code\Projects\Elevation\TestData\STRM1\N54W004.hgt");

            var data = new DecimalGeoCoordinate[1000];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (double i = 0; i < 0.9; i+= 0.01)
            {
                double a = 54;
                var elevation = fileRef.GetElevationAtCoordinate(new DecimalGeoCoordinate(a + i, -3.019312));

                Console.WriteLine($"{i}: {elevation}");
            }

            sw.Stop();
            Console.WriteLine($"{sw.Elapsed}");

            //var elevation = elevationTile1.GetElevationAtCoordinate(new DecimalGeoCoordinate(54.527747, -3.019312));

            //Console.WriteLine(elevationTile1.Location.ToString());
            //Console.WriteLine($"Test point: {elevation}");

            Console.ReadKey();
        }


    }
}