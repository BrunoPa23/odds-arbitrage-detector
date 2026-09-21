using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.OddsApi;

// Decorador que evita gastar cuota de la API en consultas repetidas
public sealed class OddsApiServiceConCache(
    OddsApiService inner,
    IMemoryCache cache,
    IOptions<OddsApiOptions> options) : IOddsApiService
{
    private readonly OddsApiOptions _opciones = options.Value;

    public async Task<IReadOnlyList<Partido>> ObtenerPartidosAsync(string? deporte = null, CancellationToken cancellationToken = default)
    {
        var claveDeporte = deporte ?? _opciones.DeportePorDefecto;

        var partidos = await cache.GetOrCreateAsync($"partidos:{claveDeporte}", entrada =>
        {
            entrada.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_opciones.MinutosCache);
            return inner.ObtenerPartidosAsync(claveDeporte, cancellationToken);
        });

        return partidos ?? [];
    }
}
