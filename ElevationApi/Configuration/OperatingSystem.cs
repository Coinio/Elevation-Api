using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Elevation.Api.Configuration;

namespace Elevation.Api.Configuration
{
    public static class OperatingSystem
    {
        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsMac() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string Name
        {
            get
            {
                if (IsWindows())
                    return "windows";
                if (IsMac())
                    return "macos";
                if (IsLinux())
                    return "linux";

                return "unknown";
            }
        }
    }
}
