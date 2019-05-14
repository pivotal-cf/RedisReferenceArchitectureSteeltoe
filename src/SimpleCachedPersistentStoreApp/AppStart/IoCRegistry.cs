using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.CloudFoundry.Connector.MySql;
using Steeltoe.Management.CloudFoundry;
using SimpleCachedPersistentStoreApp.Boundaries;
using SimpleCachedPersistentStoreApp.Services;

namespace SimpleCachedPersistentStoreApp.AppStart
{
    public class IoCRegistry
    {
        public static void RegisterDependencies(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(provider => configuration);

            // We are using the Steeltoe Redis Connector to pickup the CloudFoundry
            // Redis Service binding and use it to configure the underlying RedisCache
            // This adds a IDistributedCache to the container
            services.AddDistributedRedisCache(configuration);
            services.AddMySqlConnection(configuration);

            // Add Boundary Service
            services.AddScoped<ITokenDB, TokenDB>();

            // Add Internal Service
            services.AddScoped<ITokenService, CachedTokenRepositoryService>();

            services.AddCloudFoundryActuators(configuration);

            services.AddMvc();
        }
    }
}