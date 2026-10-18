using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.Arbitraje;

public class BuscadorSurebetsTests
{
    private readonly BuscadorSurebets _buscador = new([new DetectorArbitraje(), new DetectorDosResultados()]);

    // Partido con surebet en los tres mercados: 1X2 (5.26 %), totales (3.73 %) y handicap (2.79 %)
    private static Partido PartidoConSurebetsEnTodosLosMercados() => new PartidoBuilder()
        .ConCuota("casaA", ResultadoCuota.Local, 2.00m)
        .ConCuota("casaA", ResultadoCuota.Empate, 4.00m)
        .ConCuota("casaB", ResultadoCuota.Visitante, 5.00m)
        .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.10m)
        .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.05m)
        .ConCuota("casaA", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.60m)
        .ConCuota("casaB", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.70m)
        .Build();

    [Fact]
    public void Buscar_SinMercado_UneLosDetectoresOrdenadosPorMargen()
    {
        // Act
        var surebets = _buscador.Buscar([PartidoConSurebetsEnTodosLosMercados()]);

        // Assert
        Assert.Equal(
            [TipoMercado.Ganador, TipoMercado.Totales, TipoMercado.Handicap],
            surebets.Select(s => s.Mercado));
        Assert.Equal([5.26m, 3.73m, 2.79m], surebets.Select(s => s.MargenPorcentaje));
    }

    [Theory]
    [InlineData(TipoMercado.Ganador)]
    [InlineData(TipoMercado.Totales)]
    [InlineData(TipoMercado.Handicap)]
    public void Buscar_ConMercado_SoloDevuelveSurebetsDeEseMercado(TipoMercado mercado)
    {
        // Act
        var surebets = _buscador.Buscar([PartidoConSurebetsEnTodosLosMercados()], mercado);

        // Assert
        Assert.Equal(mercado, Assert.Single(surebets).Mercado);
    }

    [Fact]
    public void Buscar_MercadoGanador_DevuelveLoMismoQueElDetector1X2()
    {
        // Arrange
        var partidos = new[] { PartidoConSurebetsEnTodosLosMercados() };

        // Act
        var desdeBuscador = _buscador.Buscar(partidos, TipoMercado.Ganador, stakeTotal: 250m);
        var desdeDetector = new DetectorArbitraje().Detectar(partidos, stakeTotal: 250m);

        // Assert
        var esperada = Assert.Single(desdeDetector);
        var obtenida = Assert.Single(desdeBuscador);
        Assert.Equal(esperada.MargenPorcentaje, obtenida.MargenPorcentaje);
        Assert.Equal(esperada.Reparto.StakeTotal, obtenida.Reparto.StakeTotal);
        Assert.Equal(esperada.MejoresCuotas, obtenida.MejoresCuotas);
    }

    [Fact]
    public void Buscar_SinDetectoresDelMercado_DevuelveListaVacia()
    {
        // Arrange: un buscador que solo conoce el detector 1X2
        var buscador = new BuscadorSurebets([new DetectorArbitraje()]);

        // Act
        var surebets = buscador.Buscar([PartidoConSurebetsEnTodosLosMercados()], TipoMercado.Totales);

        // Assert
        Assert.Empty(surebets);
    }
}
