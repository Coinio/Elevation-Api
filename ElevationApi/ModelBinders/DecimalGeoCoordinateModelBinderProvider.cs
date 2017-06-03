using Geolocation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elevation.Api.ModelBinders
{
    public class DecimalGeoCoordinateModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(DecimalGeoCoordinate[]))
                return new BinderTypeModelBinder(typeof(DecimalGeoCoordinateArrayModelBinder));

            throw new NotImplementedException();
        }
    }
}
