using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.Entsoe.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEntsoeDataSource(this IServiceCollection services)
        {
            services.AddHttpClient<EntsoeDataSource>();
            services.TryAddSingleton<EntsoeDataSource>();
        }
    }
}