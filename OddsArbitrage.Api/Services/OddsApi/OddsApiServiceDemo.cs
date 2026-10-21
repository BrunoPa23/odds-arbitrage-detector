using System.Text.Json;
using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.OddsApi;

// Sirve partidos ficticios de un fixture versionado en lugar de llamar a The Odds API.
// No necesita API key ni gasta cuota; sirve para mostrar la app y para los tests de integracion.
// Devuelve todos los mercados del fixture (h2h, totals y spreads) sin importar TheOddsApi:Mercados
public sealed class OddsApiServiceDemo(
    TimeProvider timeProvider,
    IOptions<OddsApiOptions> options,
    ILogger<OddsApiServiceDemo> logger) : IOddsApiService
{
    internal const string RecursoFixture = "cuotas-demo.json";

    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    // El fixture va embebido en el ensamblado, asi funciona igual en dotnet run, en Docker y en los tests
    private static readonly Lazy<FixtureDemoDto> Fixture = new(CargarFixture);

    private readonly OddsApiOptions _opciones = options.Value;

    public Task<IReadOnlyList<Partido>> ObtenerPartidosAsync(string? deporte = null, CancellationToken cancellationToken = default)
    {
        var claveDeporte = deporte ?? _opciones.DeportePorDefecto;
        var fixture = Fixture.Value;

        // Mueve las fechas tantos dias como hayan pasado desde la captura, para que los partidos sigan en el futuro
        var hoy = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var desplazamiento = TimeSpan.FromDays(hoy.DayNumber - fixture.FechaCaptura.DayNumber);

        IReadOnlyList<Partido> partidos = fixture.Eventos
            .Where(e => e.SportKey == claveDeporte)
            .Select(e => Desplazar(e, desplazamiento))
            .Select(OddsApiMapper.APartido)
            .ToList();

        logger.LogInformation("Modo demo: {Cantidad} partidos de ejemplo para {Deporte}", partidos.Count, claveDeporte);

        return Task.FromResult(partidos);
    }

    private static EventDto Desplazar(EventDto evento, TimeSpan desplazamiento) => evento with
    {
        CommenceTime = evento.CommenceTime + desplazamiento,
        Bookmakers = evento.Bookmakers
            .Select(b => b with
            {
                LastUpdate = b.LastUpdate + desplazamiento,
                Markets = b.Markets.Select(m => m with { LastUpdate = m.LastUpdate + desplazamiento }).ToList()
            })
            .ToList()
    };

    private static FixtureDemoDto CargarFixture()
    {
        using var stream = typeof(OddsApiServiceDemo).Assembly.GetManifestResourceStream(RecursoFixture)
            ?? throw new InvalidOperationException($"No se encontro el recurso embebido {RecursoFixture}");

        return JsonSerializer.Deserialize<FixtureDemoDto>(stream, OpcionesJson)
            ?? throw new InvalidOperationException($"El recurso {RecursoFixture} esta vacio");
    }
}

// Envoltorio del fixture: los eventos tienen la misma forma que la respuesta de The Odds API
internal sealed record FixtureDemoDto(DateOnly FechaCaptura, IReadOnlyList<EventDto> Eventos);
