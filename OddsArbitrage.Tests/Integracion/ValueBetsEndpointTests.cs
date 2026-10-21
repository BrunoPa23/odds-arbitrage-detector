using System.Net;
using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Tests.Integracion;

public class ValueBetsEndpointTests(ApiDemoFactory factory) : IClassFixture<ApiDemoFactory>
{
    [Fact]
    public async Task Get_SinVentajaMinima_UsaElUmbralConfiguradoYOrdenaPorVentaja()
    {
        // Act
        var desfasadas = await factory.GetJsonAsync<List<CuotaDesfasada>>("/api/value-bets");

        // Assert: ValueBets:VentajaMinimaPorcentaje vale 3 en appsettings.json
        Assert.True(desfasadas.Count >= 3);
        Assert.All(desfasadas, d =>
        {
            Assert.True(d.VentajaPorcentaje >= 3m);
            Assert.True(d.CasasConsenso >= 3);
            Assert.True(d.Valor > d.CuotaJusta);
        });
        Assert.Equal(desfasadas.OrderByDescending(d => d.VentajaPorcentaje).Select(d => d.VentajaPorcentaje),
            desfasadas.Select(d => d.VentajaPorcentaje));
    }

    [Fact]
    public async Task Get_ConVentajaMinima_FiltraConEseUmbral()
    {
        // Arrange
        var porDefecto = await factory.GetJsonAsync<List<CuotaDesfasada>>("/api/value-bets");

        // Act
        var exigentes = await factory.GetJsonAsync<List<CuotaDesfasada>>("/api/value-bets?ventajaMinima=5");

        // Assert
        Assert.NotEmpty(exigentes);
        Assert.True(exigentes.Count < porDefecto.Count);
        Assert.All(exigentes, d => Assert.True(d.VentajaPorcentaje >= 5m));
    }

    [Fact]
    public async Task Get_ConVentajaMinimaCero_DevuelveTambienLasDeVentajaChica()
    {
        // Arrange
        var porDefecto = await factory.GetJsonAsync<List<CuotaDesfasada>>("/api/value-bets");

        // Act
        var todas = await factory.GetJsonAsync<List<CuotaDesfasada>>("/api/value-bets?ventajaMinima=0");

        // Assert: con umbral 0 siguen fuera las de ventaja nula o negativa
        Assert.True(todas.Count > porDefecto.Count);
        Assert.All(todas, d => Assert.True(d.VentajaPorcentaje > 0m));
    }

    [Fact]
    public async Task Get_ConMercado_SoloDevuelveCuotasDeEseMercado()
    {
        // Act
        var desfasadas = await factory.GetJsonAsync<List<CuotaDesfasada>>("/api/value-bets?mercado=Totales&ventajaMinima=1");

        // Assert
        Assert.NotEmpty(desfasadas);
        Assert.All(desfasadas, d =>
        {
            Assert.Equal(TipoMercado.Totales, d.Mercado);
            Assert.NotNull(d.Linea);
        });
    }

    [Fact]
    public async Task Get_ConDeporte_DevuelveCuotasDeEsaLiga()
    {
        // Act
        var desfasadas = await factory.GetJsonAsync<List<CuotaDesfasada>>("/api/value-bets?deporte=soccer_spain_la_liga");

        // Assert
        Assert.NotEmpty(desfasadas);
        Assert.All(desfasadas, d => Assert.Equal("La Liga - Spain", d.Liga));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("100.5")]
    [InlineData("abc")]
    public async Task Get_ConVentajaMinimaFueraDeRangoOInvalida_Devuelve400(string ventajaMinima)
    {
        // Arrange
        using var cliente = factory.CreateClient();

        // Act
        var respuesta = await cliente.GetAsync($"/api/value-bets?ventajaMinima={ventajaMinima}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Get_ConMercadoInvalido_Devuelve400()
    {
        // Arrange
        using var cliente = factory.CreateClient();

        // Act
        var respuesta = await cliente.GetAsync("/api/value-bets?mercado=Corners");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
