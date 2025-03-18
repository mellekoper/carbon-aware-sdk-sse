using CarbonAware.Exceptions;

namespace CarbonAware.DataSources.Entsoe.Configuration;

/// <summary>
/// A configuration class for holding ENTSO-E client config values.
/// </summary>
internal class EntsoeClientConfiguration
{
    /// <summary>
    /// ENTSO-E API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for ENTSO-E API
    /// </summary>
    public string BaseUrl { get; set; } = "https://web-api.tp.entsoe.eu/api";

    /// <summary>
    /// Validate that this object is properly configured.
    /// </summary>
    public void Validate()
    {
        if (!Uri.IsWellFormedUriString(this.BaseUrl, UriKind.Absolute))
        {
            throw new ConfigurationException($"{nameof(this.BaseUrl)} is not a valid absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(this.ApiKey))
        {
            throw new ConfigurationException("ENTSO-E API key is required.");
        }
    }
}
