using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CarbonAware.DataSources.Entsoe
{
    internal class EntsoeDataSource
    {
        private readonly ILogger<EntsoeDataSource> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://web-api.tp.entsoe.eu/api";

        public EntsoeDataSource(ILogger<EntsoeDataSource> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiKey = configuration["Data:Configurations:Entsoe:ApiKey"];
        }

        public async Task<XDocument> FetchCongestionDataAsync(string inDomain, string outDomain, string periodStart, string periodEnd)
        {
            var url = $"{_baseUrl}?securityToken={_apiKey}&documentType=A44&in_Domain={inDomain}&out_Domain={outDomain}&periodStart={periodStart}&periodEnd={periodEnd}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return XDocument.Parse(content);
        }

        public async Task<XDocument> FetchGenerationDataAsync(string inDomain, string periodStart, string periodEnd)
        {
            var url = $"{_baseUrl}?securityToken={_apiKey}&documentType=A75&processType=A16&in_Domain={inDomain}&periodStart={periodStart}&periodEnd={periodEnd}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return XDocument.Parse(content);
        }
    }
}