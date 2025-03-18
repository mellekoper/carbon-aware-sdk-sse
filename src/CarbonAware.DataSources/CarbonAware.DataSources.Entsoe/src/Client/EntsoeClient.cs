using CarbonAware.Model;
using CarbonAware.DataSources.Entsoe.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CarbonAware.DataSources.Entsoe.Client;

/// <summary>
/// Handles API requests to ENTSO-E.
/// </summary>
internal class EntsoeClient : IEntsoeClient
{
    private readonly HttpClient _client;
    private readonly IOptionsMonitor<EntsoeClientConfiguration> _configurationMonitor;
    private EntsoeClientConfiguration _configuration => _configurationMonitor.CurrentValue;
    private readonly ILogger<EntsoeClient> _log;

    public EntsoeClient(IHttpClientFactory factory, IOptionsMonitor<EntsoeClientConfiguration> monitor, ILogger<EntsoeClient> log)
    {
        _client = factory.CreateClient(IEntsoeClient.NamedClient);
        _configurationMonitor = monitor;
        _log = log;
        _configuration.Validate();
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string eicCode, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var formattedStartTime = startTime.ToUniversalTime().ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
        var formattedEndTime = endTime.ToUniversalTime().ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);

        // TODO: Maybe change this later!!
        string requestUrl = $"{_configuration.BaseUrl}?documentType=A71&processType=A01" +
                            $"&outBiddingZone_Domain={eicCode}" +
                            $"&periodStart={formattedStartTime}&periodEnd={formattedEndTime}" +
                            $"&securityToken={_configuration.ApiKey}";

        _log.LogInformation("Fetching data from ENTSO-E: {requestUrl}", requestUrl);

        HttpResponseMessage response = await _client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            _log.LogError("Failed to fetch data from ENTSO-E: {status}", response.StatusCode);
            throw new EntsoeClientException($"API request failed with status: {response.StatusCode}");
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        return ParseEntsoeXml(responseBody);
    }

    private IEnumerable<EmissionsData> ParseEntsoeXml(string xmlData)
    {
        XDocument xmlDoc = XDocument.Parse(xmlData);
        XNamespace ns = "urn:iec62325.351:tc57wg16:451-6:generationloaddocument:3:0";

        return xmlDoc.Descendants(ns + "Point")
                     .Select(point => new EmissionsData
                     {
                         Time = DateTimeOffset.UtcNow, // Adjust based on position
                         Rating = double.Parse(point.Element(ns + "quantity")?.Value ?? "0")
                     })
                     .ToList();
    }
}
