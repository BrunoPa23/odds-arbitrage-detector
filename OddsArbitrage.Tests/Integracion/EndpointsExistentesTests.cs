using System.Net;
using Microsoft.Extensions.DependencyInjection;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Tests.Integracion;

// Los endpoints del MVP siguen respondiendo igual sobre los datos del modo demo
public class EndpointsExistentesTests(ApiDemoFactory factory) : IClassFixture<ApiDemoFactory>
{
    [Fact]
    public void ModoDemo_ResuelveElServicioDeDatosDeEjemplo()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();

        // Act
        var servicio = scope.ServiceProvider.GetRequiredService<IOddsApiService>();

        // Assert
        Assert.IsType<OddsApiServiceDemo>(servicio);
    }

    [Theory]
    [InlineData("/api/partidos")]
    [InlineData("/api/partidos?deporte=basketball_nba")]
    [InlineData("/api/oportunidades")]
    [InlineData("/api/oportunidades?deporte=soccer_italy_serie_a&stake=250")]
    public async Task Get_EndpointsDelMvp_Devuelven200(string url)
    {
        // Arrange
        using var cliente = factory.CreateClient();

        // Act
        var respuesta = await cliente.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetPartidos_DevuelvePartidosConCuotasDeVariasCasas()
    {
        // Act
        var partidos = await factory.GetJsonAsync<List<Partido>>("/api/partidos");

        // Assert
        Assert.NotEmpty(partidos);
        Assert.All(partidos, p => Assert.True(p.Cuotas.Select(c => c.Casa.Clave).Distinct().Count() >= 4));
    }

    [Fact]
    public async Task GetOportunidades_SoloDevuelveElMercadoGanador()
    {
        // Act
        var oportunidades = await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/oportunidades");

        // Assert
        var oportunidad = Assert.Single(oportunidades);
        Assert.Equal(TipoMercado.Ganador, oportunidad.Mercado);
        Assert.Equal(3, oportunidad.Reparto.Apuestas.Count);
    }
}
