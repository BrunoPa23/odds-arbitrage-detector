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
    IBuscadorSurebets buscadorSurebets,
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
        [FromQuery, Range(typeof(decimal), "1", "1000000")] decimal stake = IDetectorSurebets.StakePorDefecto,
        CancellationToken cancellationToken = default)
    {
        var partidos = await oddsApiService.ObtenerPartidosAsync(deporte, cancellationToken);
        var oportunidades = buscadorSurebets.Buscar(partidos, TipoMercado.Ganador, stake);

        logger.LogInformation(
            "Se evaluaron {Partidos} partidos y se detectaron {Oportunidades} oportunidades",
            partidos.Count, oportunidades.Count);

        return Ok(oportunidades);
    }
}
