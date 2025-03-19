namespace CarbonAware.Interfaces;

internal interface ICongestionDataSource
{

    /// <summary>
    /// Gets the congestion for a single location for a given start and end time
    /// </summary>
    /// <param name="location">The location that should be used for getting emissions data.</param>
    /// <param name="periodStartTime">The start time of the period.</param>
    /// <param name="periodEndTime">The end time of the period.</param>
    /// <returns>A list of emissions data for the given time period.</returns>
    Task<IEnumerable<CongestionData>> GetCongestionAsync(IEnumerable<Location> location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime);

    Task<IEnumerable<CongestionData>> GetCongestionAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime);
}
