using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Api.Controllers;

[ApiController]
[Route("api/surebets")]
public class SurebetsController(
    IOddsApiService oddsApiService,
    IBuscadorSurebets buscadorSurebets,
    ILogger<SurebetsController> logger) : ControllerBase
{
    // GET /api/surebets?deporte=soccer_epl&mercado=Totales&margenMinimo=1.5&stake=100
    // Reutiliza la cache de partidos, por lo que no gasta cuota adicional de The Odds API.
    // Totales y handicap solo aparecen si estan activados en TheOddsApi:Mercados
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OportunidadArbitraje>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<IReadOnlyList<OportunidadArbitraje>>> ObtenerSurebets(
        [FromQuery, RegularExpression("^[a-z0-9_]+$")] string? deporte,
        [FromQuery, EnumDataType(typeof(TipoMercado))] TipoMercado? mercado,
        [FromQuery, Range(0, 100)] decimal? margenMinimo,
        [FromQuery, Range(1, 1_000_000)] decimal stake = IDetectorSurebets.StakePorDefecto,
        CancellationToken cancellationToken = default)
    {
        var partidos = await oddsApiService.ObtenerPartidosAsync(deporte, cancellationToken);
        var surebets = buscadorSurebets.Buscar(partidos, mercado, stake, margenMinimo);

        logger.LogInformation(
            "Se evaluaron {Partidos} partidos (mercado {Mercado}) y se detectaron {Surebets} surebets",
            partidos.Count, mercado?.ToString() ?? "todos", surebets.Count);

        return Ok(surebets);
    }
}
