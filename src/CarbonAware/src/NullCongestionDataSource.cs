using CarbonAware.Interfaces;

namespace CarbonAware;

internal class NullCongestionDataSource : ICongestionDataSource
{
    public Task<IEnumerable<CongestionData>> GetCongestionAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        throw new ArgumentException("CongestionDataSource is not configured");
    }

    public Task<IEnumerable<CongestionData>> GetCongestionAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        throw new ArgumentException("CongestionDataSource is not configured");
    }
}