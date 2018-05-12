using Geolocation;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elevation.Api.ModelBinders
{
    public class DecimalGeoCoordinateArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.BinderModelName;

            if (String.IsNullOrEmpty(modelName))
                modelName = "coordinates";

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (String.IsNullOrEmpty(value))
                return Task.CompletedTask;

            var coordinates = new List<DecimalGeoCoordinate>();
            var coordinateStrings = value.Split('|');

            foreach (var coordinateString in coordinateStrings)
            {
                string[] latAndLong = coordinateString.Split(',');

                if (latAndLong.Length != 2)
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid coordinate format.");
                    return Task.CompletedTask;
                }

                var latitudeString = latAndLong[0];
                var longitudeString = latAndLong[1];

                DecimalGeoCoordinate coordinate;

                if (!DecimalGeoCoordinate.TryParse(latitudeString, longitudeString, out coordinate))
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Invalid coordinate: {latitudeString}, {longitudeString}");
                    return Task.CompletedTask;
                }

                coordinates.Add(coordinate);
            }

            bindingContext.Result = ModelBindingResult.Success(coordinates.ToArray());

            return Task.CompletedTask;
        }
    }
}
