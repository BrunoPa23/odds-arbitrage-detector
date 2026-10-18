using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Tests.Services.OddsApi;

public class OddsApiServiceTests
{
    [Fact]
    public async Task ObtenerPartidos_SinMercadosConfigurados_PideSoloH2H()
    {
        // Arrange
        var handler = new HandlerQueCapturaRequest();
        var servicio = CrearServicio(handler, new OddsApiOptions { ApiKey = "clave-falsa" });

        // Act
        await servicio.ObtenerPartidosAsync("soccer_epl");

        // Assert
        Assert.Contains("&markets=h2h&", handler.UltimaUrl);
    }

    [Fact]
    public async Task ObtenerPartidos_ConVariosMercados_LosEnviaSeparadosPorComa()
    {
        // Arrange
        var handler = new HandlerQueCapturaRequest();
        var opciones = new OddsApiOptions { ApiKey = "clave-falsa", Mercados = ["h2h", "totals", "spreads"] };
        var servicio = CrearServicio(handler, opciones);

        // Act
        await servicio.ObtenerPartidosAsync("soccer_epl");

        // Assert: la coma va codificada como %2C
        Assert.Contains("&markets=h2h%2Ctotals%2Cspreads&", handler.UltimaUrl);
    }

    [Fact]
    public async Task ObtenerPartidos_ConTotalsEnLaRespuesta_DeserializaElPoint()
    {
        // Arrange: respuesta ficticia con la forma de The Odds API
        const string json = """
            [{
              "id": "evento-1", "sport_key": "soccer_epl", "sport_title": "Premier League",
              "commence_time": "2026-03-15T18:00:00Z", "home_team": "Equipo Local", "away_team": "Equipo Visitante",
              "bookmakers": [{
                "key": "casaA", "title": "Casa A", "last_update": "2026-03-15T10:00:00Z",
                "markets": [{
                  "key": "totals", "last_update": "2026-03-15T10:00:00Z",
                  "outcomes": [
                    { "name": "Over", "price": 1.95, "point": 2.5 },
                    { "name": "Under", "price": 1.90, "point": 2.5 }
                  ]
                }]
              }]
            }]
            """;
        var handler = new HandlerQueCapturaRequest(json);
        var opciones = new OddsApiOptions { ApiKey = "clave-falsa", Mercados = ["totals"] };
        var servicio = CrearServicio(handler, opciones);

        // Act
        var partidos = await servicio.ObtenerPartidosAsync("soccer_epl");

        // Assert
        var cuotas = Assert.Single(partidos).Cuotas;
        Assert.Equal(2, cuotas.Count);
        Assert.All(cuotas, c => Assert.Equal(2.5m, c.Linea));
        Assert.Contains(cuotas, c => c.Resultado == ResultadoCuota.Mas && c.Valor == 1.95m);
    }

    private static OddsApiService CrearServicio(HandlerQueCapturaRequest handler, OddsApiOptions opciones)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.falsa.test/") };
        return new OddsApiService(httpClient, Options.Create(opciones), NullLogger<OddsApiService>.Instance);
    }

    // Responde un JSON fijo sin salir a la red; nunca se llama a The Odds API real
    private sealed class HandlerQueCapturaRequest(string respuestaJson = "[]") : HttpMessageHandler
    {
        public string UltimaUrl { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UltimaUrl = request.RequestUri!.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(respuestaJson, Encoding.UTF8, "application/json")
            });
        }
    }
}
