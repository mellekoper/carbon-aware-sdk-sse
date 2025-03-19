using CarbonAware.WebApi.Models;
using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Route("congestion")]

public class CongestionController: ControllerBase
{
    private readonly ILogger<CongestionController> _logger;
    private readonly ICongestionHandler _congestionHandler;

    public CongestionController(ILogger<CongestionController> logger, ICongestionHandler congestionHandler)
    {
        _logger = logger;
        _congestionHandler = congestionHandler ?? throw new ArgumentNullException(nameof(congestionHandler));
    }

    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CongestionData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocation")]
    public async Task<IActionResult> GetCongestionForLoaction([FromQuery] string location, [FromQuery] DateTimeOffset startTime, [FromQuery] DateTimeOffset endTime)
    {
        _logger.LogInformation("Fetching congestion data for location {location} between {startTime} and {endTime}", location, startTime, endTime);

        var congestionData = await _congestionHandler.GetCongestionDataAsync(location);

        return Ok(congestionData);
    }

}