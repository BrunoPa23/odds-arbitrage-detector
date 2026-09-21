using System.Text.Json;
using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.OddsApi;

public sealed class OddsApiService(
    HttpClient httpClient,
    IOptions<OddsApiOptions> options,
    ILogger<OddsApiService> logger) : IOddsApiService
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly OddsApiOptions _opciones = options.Value;

    public async Task<IReadOnlyList<Partido>> ObtenerPartidosAsync(string? deporte = null, CancellationToken cancellationToken = default)
    {
        var claveDeporte = deporte ?? _opciones.DeportePorDefecto;
        var url = $"v4/sports/{Uri.EscapeDataString(claveDeporte)}/odds" +
                  $"?apiKey={Uri.EscapeDataString(_opciones.ApiKey)}" +
                  $"&regions={Uri.EscapeDataString(_opciones.Region)}" +
                  $"&markets={OddsApiMapper.MercadoH2H}" +
                  "&oddsFormat=decimal&dateFormat=iso";

        using var respuesta = await httpClient.GetAsync(url, cancellationToken);
        RegistrarCuotaRestante(respuesta);
        respuesta.EnsureSuccessStatusCode();

        var eventos = await respuesta.Content.ReadFromJsonAsync<List<EventDto>>(OpcionesJson, cancellationToken) ?? [];

        logger.LogInformation("The Odds API devolvio {Cantidad} partidos para {Deporte}", eventos.Count, claveDeporte);

        return eventos.Select(OddsApiMapper.APartido).ToList();
    }

    private void RegistrarCuotaRestante(HttpResponseMessage respuesta)
    {
        if (!respuesta.Headers.TryGetValues("x-requests-remaining", out var valores)
            || !int.TryParse(valores.FirstOrDefault(), out var restantes))
        {
            return;
        }

        if (restantes <= _opciones.UmbralAlertaCuota)
        {
            logger.LogWarning("Quedan solo {Restantes} requests de The Odds API este mes", restantes);
        }
        else
        {
            logger.LogInformation("Requests restantes de The Odds API: {Restantes}", restantes);
        }
    }
}
