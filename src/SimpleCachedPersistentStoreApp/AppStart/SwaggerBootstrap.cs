using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.Swagger;

namespace SimpleCachedPersistentStoreApp.AppStart
{
    public class SwaggerBootstrap
    {
        public static void SetupSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c => 
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1",
                new Info
                {
                    Version = "v1",
                    Title = "Simple Cached Persistent Store App",
                    Description = "An App running on top of a Redis Cache and a persistent backing store",
                    TermsOfService = "None"
                });
            });
        }

        public static void EnableSwaggerApp(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Token Service V1");
            });
        }
    }
}