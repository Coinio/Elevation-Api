# Elevation-Api
This is an api to provide elevation data similar to the google elevation api. It was mainly just an ASP.NET Core learning project. 

Swagger Documentation: 

https://elevation-api-test.azurewebsites.net/swagger/

You can provide locations (WGS84 Decimal Geocordinates) and it should give you a reasonably accurate elevation reading for those co-ordinates. It's not completely accurate due to the elevation data being of a fairly low resolution averages of the Earth's surface but is good enough for use with elevation profiles, etc.

As this is just a demo app hosted on a tiny Ubuntu vm, only the elevation data for the UK is available. If you wanted to host the project yourself, you can get the full worldwide data from here: http://wiki.openstreetmap.org/wiki/SRTM. It requires the .hgt formatted elevation files.

Example:

https://elevation-api-test.azurewebsites.net/api/elevation/56.79685,-5.003508 (Ben Nevis, Scotland) 



