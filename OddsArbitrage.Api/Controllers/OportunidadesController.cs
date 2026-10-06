using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Api.Controllers;

[ApiController]
[Route("api/oportunidades")]
public class OportunidadesController(
    IOddsApiService oddsApiService,
    IDetectorArbitraje detectorArbitraje,
    ILogger<OportunidadesController> logger) : ControllerBase
{
    // GET /api/oportunidades?deporte=soccer_epl&stake=100
    // Reutiliza la cache de partidos, por lo que no gasta cuota adicional de The Odds API
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OportunidadArbitraje>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<IReadOnlyList<OportunidadArbitraje>>> ObtenerOportunidades(
        [FromQuery, RegularExpression("^[a-z0-9_]+$")] string? deporte,
        [FromQuery, Range(1, 1_000_000)] decimal stake = DetectorArbitraje.StakePorDefecto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var partidos = await oddsApiService.ObtenerPartidosAsync(deporte, cancellationToken);
            var oportunidades = detectorArbitraje.Detectar(partidos, stake);

            logger.LogInformation(
                "Se evaluaron {Partidos} partidos y se detectaron {Oportunidades} oportunidades",
                partidos.Count, oportunidades.Count);

            return Ok(oportunidades);
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
