using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.DataSources.Entsoe.Client;
// using CarbonAware.DataSources.Entsoe.Configuration;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace CarbonAware.DataSources.Entsoe.Configuration;

/// <summary>
/// Dependency injection configuration for ENTSO-E.
/// </summary>
internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntsoeCongestionDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddEntsoeClient(services, dataSourcesConfig.CongestionFigurationSection());
        services.TryAddSingleton<ICongestionDataSource, EntsoeDataSource>();
        return services;
    }

    private static void AddEntsoeClient(IServiceCollection services, IConfigurationSection configSection)
    {
        services.Configure<EntsoeClientConfiguration>(c =>
        {
            configSection.Bind(c);
        });

        var httpClientBuilder = services.AddHttpClient<IEntsoeClient, EntsoeClient>(IEntsoeClient.NamedClient);

        var proxy = configSection.GetSection("Proxy").Get<WebProxyConfiguration>();
        if (proxy?.UseProxy == true)
        {
            if (string.IsNullOrEmpty(proxy.Url))
            {
                throw new ConfigurationException("Proxy URL is not configured.");
            }

            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler()
                {
                    Proxy = new WebProxy
                    {
                        Address = new Uri(proxy.Url),
                        Credentials = new NetworkCredential(proxy.Username, proxy.Password),
                        BypassProxyOnLocal = true
                    }
                });
        }

        services.TryAddSingleton<IEntsoeClient, EntsoeClient>();
    }
}
