using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Management.CloudFoundry;

namespace SimpleCachedPersistentStoreApp.AppStart
{
    public class Startup
    {
        public IConfiguration AppConfig { get; }

        public Startup(IConfiguration configuration)
        {
            AppConfig = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IoCRegistry.RegisterDependencies(services, AppConfig);
            SwaggerBootstrap.SetupSwagger(services);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCloudFoundryActuators();

            SwaggerBootstrap.EnableSwaggerApp(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
