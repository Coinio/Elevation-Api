using Elevation.Api.Configuration;
using Elevation.Api.ModelBinders;
using Elevation.Data.Files;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;

namespace ElevationApi
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;

            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: false)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ElevationDataOptions>(Configuration);

            services.AddScoped<IElevationFileReader, ElevationHgtFileReader>();

            services.AddMemoryCache();
            services.AddMvc(options =>
            {
                options.ModelBinderProviders.Insert(0, new DecimalGeoCoordinateModelBinderProvider());
            });

            var xmlCommentsPath = GetXmlCommentsPath();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Elevation Api", Version = "v1" });
                c.IncludeXmlComments(xmlCommentsPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elevation Api V1");
            });
        }

        private String GetXmlCommentsPath()
        {
            var appPath = _hostingEnvironment.WebRootPath;
            
            return Path.Combine(appPath, "elevationapi.xml");
        }
    }
}
