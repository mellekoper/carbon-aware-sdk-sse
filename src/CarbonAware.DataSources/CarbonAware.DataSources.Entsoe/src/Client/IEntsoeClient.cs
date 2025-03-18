using CarbonAware.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.Entsoe.Client;

/// <summary>
/// An interface for interacting with the ENTSO-E API.
/// </summary>
internal interface IEntsoeClient
{
    public const string NamedClient = "EntsoeClient";

    /// <summary>
    /// Retrieves observed emission data for a given EIC code over a specified time period.
    /// </summary>
    /// <param name="eicCode">EIC code representing the grid region</param>
    /// <param name="startTime">Start time for query</param>
    /// <param name="endTime">End time for query</param>
    /// <returns>A <see cref="Task{IEnumerable{EmissionsData}}"/> containing emissions data points.</returns>
    Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string eicCode, DateTimeOffset startTime, DateTimeOffset endTime);
}
