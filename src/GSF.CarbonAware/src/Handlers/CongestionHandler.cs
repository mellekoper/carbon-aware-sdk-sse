using System.Globalization;
using CarbonAware.DataSources.Entsoe.Client;
using CarbonAware.Exceptions;
using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using static CarbonAware.Model.CarbonAwareParameters;
using CongestionData = GSF.CarbonAware.Models.CongestionData;

namespace GSF.CarbonAware.Handlers;

internal sealed class CongestionHandler : ICongestionHandler
{
        private readonly ILogger<CongestionHandler> _logger;
        private readonly ICongestionDataSource _congestionDataSource;

        public CongestionHandler(ILogger<CongestionHandler> logger, ICongestionDataSource congestionDataSource)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _congestionDataSource = congestionDataSource ?? throw new ArgumentNullException(nameof(congestionDataSource));
        }

        public async Task<IEnumerable<CongestionData>> GetCongestionDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
        {
            return await GetCongestionDataAsync(new string[] { location }, start, end);
        }

    public async Task<IEnumerable<CongestionData>> GetCongestionDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
        };

        var parameters = (CarbonAwareParameters)dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.SetValidations(ValidationName.StartRequiredIfEnd);
            parameters.Validate();

            var multipleLocations = parameters.MultipleLocations;
            var startTime = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
            var endTime = parameters.GetEndOrDefault(startTime);

            var CongestionData = await _congestionDataSource.GetCongestionAsync(multipleLocations, startTime, endTime);

            return CongestionData.Select(e => (CongestionData) e);
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }
       public async Task<IEnumerable<CongestionData>> GetBestCongestionDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        return await GetBestCongestionDataAsync(new string[] { location }, start, end);   
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<CongestionData>> GetBestCongestionDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
        };

        var parameters = (CarbonAwareParameters)dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.SetValidations(ValidationName.StartRequiredIfEnd);
            parameters.Validate();

            var startTime = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
            var endTime = parameters.GetEndOrDefault(startTime);
            var results = await _congestionDataSource.GetCongestionAsync(parameters.MultipleLocations, startTime, endTime);
            var congestion = CarbonAwareOptimalCongestion.GetOptimalCongestion(results).Select(r => (CongestionData)r);
            return congestion;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }
    public async Task<(double, double, double)> GetAverageCongestionAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            SingleLocation = location,
        };

        var parameters = (CarbonAwareParameters)dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.SingleLocation);
            parameters.SetValidations(ValidationName.StartRequiredIfEnd);
            parameters.Validate();
            int weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            WeekPeriod weekPeriod = GetWeekPeriod(DateTime.UtcNow.Year, weekNumber);
            start = parameters.GetStartOrDefault(DateTimeOffset.Parse(weekPeriod.PeriodStart));
            end = parameters.GetEndOrDefault(DateTimeOffset.Parse(weekPeriod.PeriodEnd));

            _logger.LogInformation("Handler getting average congestion from data source");
            var congestionData = await _congestionDataSource.GetCongestionAsync(parameters.SingleLocation, (DateTimeOffset) start, (DateTimeOffset) end);
            Console.WriteLine("congestion query done");
            var value = congestionData.AverageOverPeriod((DateTimeOffset)start, (DateTimeOffset) end);
            _logger.LogInformation("Congestion Average: {value}", value);

            return value;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
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
            PeriodStart = periodStart.ToString("dd MMM yyyy"),
            PeriodEnd = periodEnd.ToString("dd MMM yyyy")
        };
    }
}