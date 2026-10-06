using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Api.Controllers;

[ApiController]
[Route("api/partidos")]
public class PartidosController(IOddsApiService oddsApiService) : ControllerBase
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
        => Ok(await oddsApiService.ObtenerPartidosAsync(deporte, cancellationToken));
}
