using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Api.Controllers;

[ApiController]
[Route("api/partidos")]
public class PartidosController(
    IOddsApiService oddsApiService,
    ILogger<PartidosController> logger) : ControllerBase
{
    // GET /api/partidos?deporte=soccer_epl
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Partido>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<IReadOnlyList<Partido>>> ObtenerPartidos(
        [FromQuery, RegularExpression("^[a-z0-9_]+$")] string? deporte,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await oddsApiService.ObtenerPartidosAsync(deporte, cancellationToken));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The Odds API respondio con error ({StatusCode})", ex.StatusCode);
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "No se pudieron obtener las cuotas de The Odds API");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError("The Odds API no respondio a tiempo");
            return Problem(
                statusCode: StatusCodes.Status504GatewayTimeout,
                title: "The Odds API no respondio a tiempo");
        }
    }
}
