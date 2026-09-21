using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.OddsApi;

public interface IOddsApiService
{
    // Cada llamada consume 1 request de la cuota mensual (1 region x 1 mercado)
    Task<IReadOnlyList<Partido>> ObtenerPartidosAsync(string? deporte = null, CancellationToken cancellationToken = default);
}
