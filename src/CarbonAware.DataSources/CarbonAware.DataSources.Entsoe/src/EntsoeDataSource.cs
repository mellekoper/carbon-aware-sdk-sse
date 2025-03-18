using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.DataSources.Entsoe.Client;
using Microsoft.Extensions.Logging;

namespace CarbonAware.DataSources.Entsoe;

/*
macOS/Linux I think:
export DataSources__EmissionsDataSource="Entsoe"
export DataSources__Configurations__EntsoE__ApiKey="YOUR_ENTSOE_API_KEY"

Powershell:
$env:DataSources__EmissionsDataSource="Entsoe"
$env:DataSources__Configurations__EntsoE__ApiKey="<YOUR_ENTSOE_API_KEY>"
*/

internal class EntsoeDataSource : IEmissionsDataSource
{
    private readonly EntsoeClient _client;
    private readonly ILogger<EntsoeDataSource> _logger;

    public EntsoeDataSource(EntsoeClient client, ILogger<EntsoeDataSource> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var emissionsData = new List<EmissionsData>();

        foreach (var location in locations)
        {
            string eicCode = ConvertLocationToEIC(location.Name);
            var data = await _client.GetEmissionsDataAsync(eicCode, periodStartTime, periodEndTime);
            emissionsData.AddRange(data);
        }

        return emissionsData;
    }

    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var emissionsData = new List<EmissionsData>();

        string eicCode = ConvertLocationToEIC(location.Name);
        var data = await _client.GetEmissionsDataAsync(eicCode, periodStartTime, periodEndTime);
        emissionsData.AddRange(data);

        return emissionsData;
    }

    private string ConvertLocationToEIC(string locationName)
    {
        var eicCodes = new Dictionary<string, string>
        {
            { "Estonia", "10Y1001A1001A39I" },
            { "Denmark", "10Y1001A1001A65H" },
            { "Germany", "10Y1001A1001A83F" },
            { "United Kingdom", "10Y1001A1001A92E" },
            { "Malta", "10Y1001A1001A93C" },
            { "Moldova", "10Y1001A1001A990" },
            { "Armenia", "10Y1001A1001B004" },
            { "Georgia", "10Y1001A1001B012" },
            { "Azerbaijan", "10Y1001A1001B05V" },
            { "Ukraine", "10Y1001C--00003F" },
            { "Kosovo", "10Y1001C--00100H" },
            { "Albania", "10YAL-KESH-----5" },
            { "Austria", "10YAT-APG------L" },
            { "Bosnia and Herzegovina", "10YBA-JPCC-----D" },
            { "Belgium", "10YBE----------2" },
            { "Bulgaria", "10YCA-BULGARIA-R" },
            { "Switzerland", "10YCH-SWISSGRIDZ" },
            { "Serbia", "10YCS-CG-TSO---S" },
            { "Cyprus", "10YCY-1001A0003J" },
            { "Czech Republic", "10YCZ-CEPS-----N" },
            { "Spain", "10YES-REE------0" },
            { "Finland", "10YFI-1--------U" },
            { "France", "10YFR-RTE------C" },
            { "Greece", "10YGR-HTSO-----Y" },
            { "Croatia", "10YHR-HEP------M" },
            { "Hungary", "10YHU-MAVIR----U" },
            { "Ireland", "10YIE-1001A00010" },
            { "Italy", "10YIT-GRTN-----B" },
            { "Lithuania", "10YLT-1001A0008Q" },
            { "Luxembourg", "10YLU-CEGEDEL-NQ" },
            { "Latvia", "10YLV-1001A00074" },
            { "North Macedonia", "10YMK-MEPSO----8" },
            { "Netherlands", "10YNL----------L" },
            { "Norway", "10YNO-0--------C" },
            { "Poland", "10YPL-AREA-----S" },
            { "Portugal", "10YPT-REN------W" },
            { "Romania", "10YRO-TEL------P" },
            { "Sweden", "10YSE-1--------K" },
            { "Slovenia", "10YSI-ELES-----O" },
            { "Slovakia", "10YSK-SEPS-----K" },
            { "Turkey", "10YTR-TEIAS----W" },
            { "Belarus", "BY" },
            { "Iceland", "IS" },
            { "Russia", "RU" }
        };

        if (eicCodes.TryGetValue(locationName, out string eicCode))
        {
            return eicCode;
        }

        throw new ArgumentException($"Unknown location: {locationName}");
    }
}
