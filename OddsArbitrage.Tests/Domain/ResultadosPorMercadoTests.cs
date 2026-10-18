using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Tests.Domain;

public class ResultadosPorMercadoTests
{
    [Fact]
    public void De_Ganador_DevuelveLosTresResultadosDel1X2()
    {
        // Act
        var resultados = ResultadosPorMercado.De(TipoMercado.Ganador);

        // Assert
        Assert.Equal([ResultadoCuota.Local, ResultadoCuota.Empate, ResultadoCuota.Visitante], resultados);
    }

    [Fact]
    public void De_Totales_DevuelveMasYMenos()
    {
        // Act
        var resultados = ResultadosPorMercado.De(TipoMercado.Totales);

        // Assert
        Assert.Equal([ResultadoCuota.Mas, ResultadoCuota.Menos], resultados);
    }

    [Fact]
    public void De_Handicap_DevuelveLocalYVisitanteSinEmpate()
    {
        // Act
        var resultados = ResultadosPorMercado.De(TipoMercado.Handicap);

        // Assert
        Assert.Equal([ResultadoCuota.Local, ResultadoCuota.Visitante], resultados);
    }

    [Theory]
    [InlineData(TipoMercado.Ganador, ResultadoCuota.Mas, false)]
    [InlineData(TipoMercado.Totales, ResultadoCuota.Local, false)]
    [InlineData(TipoMercado.Handicap, ResultadoCuota.Empate, false)]
    [InlineData(TipoMercado.Totales, ResultadoCuota.Menos, true)]
    [InlineData(TipoMercado.Handicap, ResultadoCuota.Visitante, true)]
    public void EsValido_ValidaElResultadoSegunElMercado(TipoMercado mercado, ResultadoCuota resultado, bool esperado)
    {
        // Act y Assert
        Assert.Equal(esperado, ResultadosPorMercado.EsValido(mercado, resultado));
    }
}
