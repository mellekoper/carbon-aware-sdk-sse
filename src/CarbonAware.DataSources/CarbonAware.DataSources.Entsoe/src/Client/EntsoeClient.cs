using CarbonAware.Model;
using CarbonAware.DataSources.Entsoe.Configuration;
using CarbonAware.DataSources.Entsoe.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Security.Authentication;
using System.ComponentModel.DataAnnotations;

namespace CarbonAware.DataSources.Entsoe.Client;

/// <summary>
/// Handles API requests to ENTSO-E.
/// </summary>

    internal class EntsoeClient : IEntsoeClient
    {
        public const string NamedClient = "EntsoeClient";
        private readonly HttpClient _client;
        private readonly IOptionsMonitor<EntsoeClientConfiguration> _configurationMonitor;
        private EntsoeClientConfiguration _configuration => _configurationMonitor.CurrentValue;
        private readonly ILogger<EntsoeClient> _log;

        public EntsoeClient(HttpClient client, IOptionsMonitor<EntsoeClientConfiguration> monitor, ILogger<EntsoeClient> log)
        {
            _client = client;
            _configurationMonitor = monitor;
            _log = log;
            _configuration.Validate();
        }

        public async Task<IEnumerable<CongestionData>> GetCongestionDataAsync(string eicCode, string country, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            // var formattedStartTime = startTime.ToUniversalTime().ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
            // var formattedEndTime = endTime.ToUniversalTime().ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
            // var formattedStartTime = "202501192300";
            // var formattedEndTime = "202501262300";
            int weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            WeekPeriod weekPeriod = GetWeekPeriod(startTime.Year, weekNumber);
            var formattedStartTime = weekPeriod.PeriodStart;
            var formattedEndTime = weekPeriod.PeriodEnd;

            string requestUrlGeneration = $"{_configuration.BaseUrl}?documentType=A71&processType=A01" +
                                $"&in_Domain={eicCode}" +
                                $"&periodStart={formattedStartTime}&periodEnd={formattedEndTime}" +
                                $"&securityToken={_configuration.ApiKey}";
            _log.LogInformation("Fetching data from ENTSO-E: {requestUrl}", requestUrlGeneration);

            HttpResponseMessage responseGeneration = await _client.GetAsync(requestUrlGeneration);
            if (!responseGeneration.IsSuccessStatusCode)
            {
                _log.LogError("Failed to fetch data from ENTSO-E: {status}", responseGeneration.StatusCode);
                throw new EntsoeClientException($"API request failed with status: {responseGeneration.StatusCode}");
            }

            string responseBodyGeneration = await responseGeneration.Content.ReadAsStringAsync();


            string requestUrlLoad = $"{_configuration.BaseUrl}?documentType=A65&processType=A16" +
                    $"&outBiddingZone_Domain={eicCode}" +
                    $"&periodStart={formattedStartTime}&periodEnd={formattedEndTime}" +
                    $"&securityToken={_configuration.ApiKey}";
            _log.LogInformation("Fetching data from ENTSO-E: {requestUrl}", requestUrlLoad);

            HttpResponseMessage responseLoad = await _client.GetAsync(requestUrlLoad);
            if (!responseLoad.IsSuccessStatusCode)
            {
                _log.LogError("Failed to fetch data from ENTSO-E: {status}", responseLoad.StatusCode);
                throw new EntsoeClientException($"API request failed with status: {responseLoad.StatusCode}");
            }

            string responseBodyLoad = await responseLoad.Content.ReadAsStringAsync();

            return ParseEntsoeXml(responseBodyGeneration, responseBodyLoad, country, ParseDateTime(weekPeriod.PeriodStart));
        }
        private IEnumerable<CongestionData> ParseEntsoeXml(string xmlDataGeneration, string xmlDataLoad, string country, DateTimeOffset startTime)
        {
            XDocument xmlDocGeneration = XDocument.Parse(xmlDataGeneration);
            XDocument xmlDocLoad = XDocument.Parse(xmlDataLoad);
            XNamespace ns = "urn:iec62325.351:tc57wg16:451-6:generationloaddocument:3:0";

            List<int> generationList = xmlDocGeneration.Descendants(ns + "Point")
                .Select(point => int.Parse(point.Element(ns + "quantity")?.Value ?? "0"))
                .ToList();

            List<int> loadList = xmlDocLoad.Descendants(ns + "Point")
                .Select(point => int.Parse(point.Element(ns + "quantity")?.Value ?? "0"))
                .ToList();

            Dictionary<int, int> generation = generationList
                .Select((value, index) => new { value, index })
                .ToDictionary(item => item.index, item => item.value);

            Dictionary<int, int> load = loadList
                .Select((value, index) => new { value, index })
                .ToDictionary(item => item.index, item => item.value);

            List<CongestionData> congestionData = new List<CongestionData>();
            int count = Math.Min(generation.Count, load.Count);

            for (int i = 0; i < count; i++)
            {
                DateTimeOffset time = startTime.AddMinutes(i * 15);
                congestionData.Add(new CongestionData
                {
                    Location = country,
                    Time = time,
                    Generation = generation[i],
                    Load = load[i],
                    Difference = generation[i] - load[i]
                });
            }

            return congestionData;
        }

    public WeekPeriod GetWeekPeriod(int year, int weekNumber)
    {
        // Find Monday of the given ISO week
        DateTime firstDayOfYear = new DateTime(year, 1, 4);  // Jan 4 ensures we are in the first ISO week
        DateTime monday = firstDayOfYear.AddDays((weekNumber - 1) * 7 - (int)firstDayOfYear.DayOfWeek + 1);

        // Adjust to previous Sunday 23:00 UTC (for periodStart)
        DateTime periodStart = monday.AddDays(-1);  // Previous Sunday
        periodStart = new DateTime(periodStart.Year, periodStart.Month, periodStart.Day, 23, 0, 0, DateTimeKind.Utc);

        // End of the selected week (Sunday 23:00 UTC of the NEXT week)
        DateTime periodEnd = periodStart.AddDays(7);

        return new WeekPeriod
        {
            Label = $"Week {weekNumber}, {year}: {periodStart.ToString("dd MMM yyyy")} - {periodEnd.ToString("dd MMM yyyy")}",
            PeriodStart = periodStart.ToString("yyyyMMddHHmm"),
            PeriodEnd = periodEnd.ToString("yyyyMMddHHmm")
        };
    }
    public DateTime ParseDateTime(string dateTimeString)
    {
        return DateTime.ParseExact(dateTimeString, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
    }
}

public class WeekPeriod
{
    public string Label { get; set; }
    public string PeriodStart { get; set; }
    public string PeriodEnd { get; set; }
}