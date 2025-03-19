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
    public async Task<double> GetAverageCongestionAsync(string location, DateTimeOffset start, DateTimeOffset end)
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
            parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
            parameters.Validate();

            _logger.LogInformation("Handler getting average congestion from data source");
            var congestionData = await _congestionDataSource.GetCongestionAsync(parameters.SingleLocation, parameters.Start, parameters.End);
            var value = congestionData.AverageOverPeriod(start, end);
            _logger.LogInformation("Congestion Average: {value}", value);

            return value;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }
}