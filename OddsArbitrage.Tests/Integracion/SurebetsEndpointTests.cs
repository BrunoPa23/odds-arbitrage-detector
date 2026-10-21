using System.Net;
using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Tests.Integracion;

public class SurebetsEndpointTests(ApiDemoFactory factory) : IClassFixture<ApiDemoFactory>
{
    [Fact]
    public async Task Get_SinFiltros_DevuelveSurebetsDeLosTresMercadosPorMargenDescendente()
    {
        // Act
        var surebets = await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/surebets");

        // Assert: la liga por defecto del fixture tiene una surebet en cada mercado
        Assert.Equal(
            [TipoMercado.Ganador, TipoMercado.Totales, TipoMercado.Handicap],
            surebets.Select(s => s.Mercado).Order());
        Assert.Equal(surebets.OrderByDescending(s => s.MargenPorcentaje).Select(s => s.MargenPorcentaje),
            surebets.Select(s => s.MargenPorcentaje));
        Assert.All(surebets, s =>
        {
            Assert.True(s.SumaProbabilidadesImplicitas < 1m);
            Assert.True(s.Reparto.GananciaGarantizada > 0m);
        });
    }

    [Theory]
    [InlineData(TipoMercado.Ganador)]
    [InlineData(TipoMercado.Totales)]
    [InlineData(TipoMercado.Handicap)]
    public async Task Get_ConMercado_SoloDevuelveSurebetsDeEseMercado(TipoMercado mercado)
    {
        // Act
        var surebets = await factory.GetJsonAsync<List<OportunidadArbitraje>>($"/api/surebets?mercado={mercado}");

        // Assert
        Assert.NotEmpty(surebets);
        Assert.All(surebets, s => Assert.Equal(mercado, s.Mercado));
    }

    [Fact]
    public async Task Get_ConMercadoTotales_DevuelveLaLineaYLasCuotasDeDistintasCasas()
    {
        // Act
        var surebet = Assert.Single(await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/surebets?mercado=Totales"));

        // Assert
        Assert.Equal(2.5m, surebet.Linea);
        Assert.Equal([ResultadoCuota.Mas, ResultadoCuota.Menos], surebet.MejoresCuotas.Select(c => c.Resultado));
        Assert.Equal(2, surebet.MejoresCuotas.Select(c => c.Casa.Clave).Distinct().Count());
    }

    [Fact]
    public async Task Get_ConMargenMinimo_DescartaLasDeMenorMargen()
    {
        // Arrange
        var todas = await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/surebets");

        // Act
        var filtradas = await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/surebets?margenMinimo=2");

        // Assert
        Assert.NotEmpty(filtradas);
        Assert.True(filtradas.Count < todas.Count);
        Assert.All(filtradas, s => Assert.True(s.MargenPorcentaje >= 2m));
    }

    [Fact]
    public async Task Get_ConMercadoYMargenMinimo_AplicaAmbosFiltros()
    {
        // Act
        var surebets = await factory.GetJsonAsync<List<OportunidadArbitraje>>(
            "/api/surebets?deporte=soccer_germany_bundesliga&mercado=Totales&margenMinimo=2.5");

        // Assert
        var surebet = Assert.Single(surebets);
        Assert.Equal(TipoMercado.Totales, surebet.Mercado);
        Assert.Equal(3.5m, surebet.Linea);
        Assert.True(surebet.MargenPorcentaje >= 2.5m);
    }

    [Fact]
    public async Task Get_ConMargenMinimoMuyAlto_DevuelveListaVacia()
    {
        // Act
        var surebets = await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/surebets?margenMinimo=50");

        // Assert
        Assert.Empty(surebets);
    }

    [Fact]
    public async Task Get_ConDeporteNba_DevuelveLaSurebetDeHandicapConLineaDelLocal()
    {
        // Act
        var surebets = await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/surebets?deporte=basketball_nba");

        // Assert: el visitante cotiza -4.5 y se empareja con el local +4.5
        var surebet = Assert.Single(surebets);
        Assert.Equal(TipoMercado.Handicap, surebet.Mercado);
        Assert.Equal(4.5m, surebet.Linea);
    }

    [Fact]
    public async Task Get_ConStake_RepartoUsaEseStake()
    {
        // Act
        var surebets = await factory.GetJsonAsync<List<OportunidadArbitraje>>("/api/surebets?stake=1000");

        // Assert: el stake repartido puede diferir en centavos por el redondeo
        Assert.All(surebets, s => Assert.InRange(s.Reparto.StakeTotal, 999.95m, 1000.05m));
    }

    [Theory]
    [InlineData("/api/surebets?mercado=Corners")]
    [InlineData("/api/surebets?margenMinimo=150")]
    [InlineData("/api/surebets?margenMinimo=-1")]
    [InlineData("/api/surebets?margenMinimo=100.5")]
    [InlineData("/api/surebets?stake=0.6")]
    [InlineData("/api/surebets?stake=0")]
    [InlineData("/api/surebets?deporte=Futbol%20Ingles")]
    public async Task Get_ConParametrosInvalidos_Devuelve400(string url)
    {
        // Arrange
        using var cliente = factory.CreateClient();

        // Act
        var respuesta = await cliente.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
