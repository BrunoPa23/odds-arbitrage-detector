using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.OddsApi;

public interface IOddsApiService
{
    // Cada consulta real a la API consume (regiones x mercados) requests de la cuota mensual; con h2h solo es 1
    Task<IReadOnlyList<Partido>> ObtenerPartidosAsync(string? deporte = null, CancellationToken cancellationToken = default);
}
