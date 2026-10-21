using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.OddsApi;
using OddsArbitrage.Api.Services.ValueBets;

namespace OddsArbitrage.Api.Controllers;

[ApiController]
[Route("api/value-bets")]
public class ValueBetsController(
    IOddsApiService oddsApiService,
    IDetectorCuotasDesfasadas detector,
    IOptions<ValueBetsOptions> options,
    ILogger<ValueBetsController> logger) : ControllerBase
{
    // GET /api/value-bets?deporte=soccer_epl&mercado=Ganador&ventajaMinima=3
    // Reutiliza la cache de partidos, por lo que no gasta cuota adicional de The Odds API.
    // Sin ventajaMinima se usa ValueBets:VentajaMinimaPorcentaje; el minimo de casas sale de ValueBets:MinimoCasas
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CuotaDesfasada>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<IReadOnlyList<CuotaDesfasada>>> ObtenerValueBets(
        [FromQuery, RegularExpression("^[a-z0-9_]+$")] string? deporte,
        [FromQuery, EnumDataType(typeof(TipoMercado))] TipoMercado? mercado,
        [FromQuery, Range(0, 100)] decimal? ventajaMinima,
        CancellationToken cancellationToken = default)
    {
        var opciones = options.Value;
        var umbral = ventajaMinima ?? opciones.VentajaMinimaPorcentaje;

        var partidos = await oddsApiService.ObtenerPartidosAsync(deporte, cancellationToken);
        var desfasadas = detector.Detectar(partidos, mercado, umbral, opciones.MinimoCasas);

        logger.LogInformation(
            "Se evaluaron {Partidos} partidos (mercado {Mercado}, ventaja minima {Umbral} %) y se detectaron {Desfasadas} cuotas desfasadas",
            partidos.Count, mercado?.ToString() ?? "todos", umbral, desfasadas.Count);

        return Ok(desfasadas);
    }
}
