using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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

    private static OddsApiService CrearServicio(HandlerQueCapturaRequest handler, OddsApiOptions opciones)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.falsa.test/") };
        return new OddsApiService(httpClient, Options.Create(opciones), NullLogger<OddsApiService>.Instance);
    }

    // Responde una lista vacia sin salir a la red; nunca se llama a The Odds API real
    private sealed class HandlerQueCapturaRequest : HttpMessageHandler
    {
        public string UltimaUrl { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UltimaUrl = request.RequestUri!.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });
        }
    }
}
