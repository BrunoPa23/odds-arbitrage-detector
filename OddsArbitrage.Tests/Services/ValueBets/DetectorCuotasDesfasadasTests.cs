using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.ValueBets;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.ValueBets;

public class DetectorCuotasDesfasadasTests
{
    private readonly DetectorCuotasDesfasadas _detector = new();

    // Consenso de cuatro casas: local 0.465102, empate 0.280645 y visitante 0.254253.
    // casaD paga 2.30 al local (ventaja 6.97 %) y casaC paga 4.20 al visitante (ventaja 6.79 %)
    private static PartidoBuilder Partido1X2(string id = "partido-1x2") => new PartidoBuilder()
        .ConId(id)
        .ConCuota("casaA", ResultadoCuota.Local, 2.00m)
        .ConCuota("casaA", ResultadoCuota.Empate, 3.50m)
        .ConCuota("casaA", ResultadoCuota.Visitante, 4.00m)
        .ConCuota("casaB", ResultadoCuota.Local, 2.10m)
        .ConCuota("casaB", ResultadoCuota.Empate, 3.40m)
        .ConCuota("casaB", ResultadoCuota.Visitante, 3.60m)
        .ConCuota("casaC", ResultadoCuota.Local, 1.95m)
        .ConCuota("casaC", ResultadoCuota.Empate, 3.60m)
        .ConCuota("casaC", ResultadoCuota.Visitante, 4.20m)
        .ConCuota("casaD", ResultadoCuota.Local, 2.30m)
        .ConCuota("casaD", ResultadoCuota.Empate, 3.30m)
        .ConCuota("casaD", ResultadoCuota.Visitante, 3.50m);

    // Totales 2.5 con consenso 0.493242 para el mas; casaD paga 2.10 al mas (ventaja 3.58 %)
    private static PartidoBuilder ConTotales(PartidoBuilder builder) => builder
        .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.90m)
        .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.90m)
        .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.85m)
        .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.95m)
        .ConCuota("casaC", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.88m)
        .ConCuota("casaC", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.92m)
        .ConCuota("casaD", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.10m)
        .ConCuota("casaD", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.75m);

    // Handicap local -1.5 contra visitante +1.5; casaD paga 1.72 al visitante (ventaja 4.74 %)
    private static PartidoBuilder ConHandicap(PartidoBuilder builder) => builder
        .ConCuota("casaA", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.50m)
        .ConCuota("casaA", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.55m)
        .ConCuota("casaB", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.45m)
        .ConCuota("casaB", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.57m)
        .ConCuota("casaC", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.55m)
        .ConCuota("casaC", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.52m)
        .ConCuota("casaD", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.40m)
        .ConCuota("casaD", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.72m);

    [Fact]
    public void Detectar_1X2_DevuelveLasCuotasPorEncimaDelUmbralOrdenadasPorVentaja()
    {
        // Act
        var desfasadas = _detector.Detectar([Partido1X2().Build()]);

        // Assert
        Assert.Collection(desfasadas,
            local =>
            {
                Assert.Equal("casaD", local.Casa.Clave);
                Assert.Equal(ResultadoCuota.Local, local.Resultado);
                Assert.Equal(TipoMercado.Ganador, local.Mercado);
                Assert.Null(local.Linea);
                Assert.Equal(2.30m, local.Valor);
                Assert.Equal(0.465102m, local.ProbabilidadJusta);
                Assert.Equal(2.150m, local.CuotaJusta);
                Assert.Equal(6.97m, local.VentajaPorcentaje);
                Assert.Equal(4, local.CasasConsenso);
                Assert.Equal("partido-1x2", local.PartidoId);
            },
            visitante =>
            {
                Assert.Equal("casaC", visitante.Casa.Clave);
                Assert.Equal(ResultadoCuota.Visitante, visitante.Resultado);
                Assert.Equal(3.933m, visitante.CuotaJusta);
                Assert.Equal(6.79m, visitante.VentajaPorcentaje);
            });
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(1.70, 3)]
    [InlineData(6.79, 2)]
    [InlineData(7, 0)]
    public void Detectar_ConVentajaMinima_DescartaLasDeMenorVentaja(decimal ventajaMinima, int esperadas)
    {
        // Act
        var desfasadas = _detector.Detectar([Partido1X2().Build()], ventajaMinima: ventajaMinima);

        // Assert: con umbral 0 solo entran las cuatro cuotas con ventaja positiva, nunca las negativas
        Assert.Equal(esperadas, desfasadas.Count);
        Assert.All(desfasadas, d => Assert.True(d.VentajaPorcentaje > 0m && d.VentajaPorcentaje >= ventajaMinima));
    }

    [Fact]
    public void Detectar_LaVentajaEsCuotaPorProbabilidadJustaMenosUno()
    {
        // Act
        var desfasadas = _detector.Detectar([Partido1X2().Build()], ventajaMinima: 0m);

        // Assert: la cuota justa supera a la que ofrece la casa solo si no hay ventaja
        Assert.All(desfasadas, d =>
        {
            Assert.Equal(Math.Round((d.Valor * d.ProbabilidadJusta - 1m) * 100m, 2), d.VentajaPorcentaje);
            Assert.True(d.Valor > d.CuotaJusta);
        });
    }

    [Fact]
    public void Detectar_MenosCasasQueElMinimo_NoDevuelveNada()
    {
        // Arrange: el mismo desfase pero con solo dos casas
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoCuota.Local, 2.00m)
            .ConCuota("casaA", ResultadoCuota.Empate, 3.50m)
            .ConCuota("casaA", ResultadoCuota.Visitante, 4.00m)
            .ConCuota("casaD", ResultadoCuota.Local, 2.30m)
            .ConCuota("casaD", ResultadoCuota.Empate, 3.30m)
            .ConCuota("casaD", ResultadoCuota.Visitante, 3.50m)
            .Build();

        // Act y Assert
        Assert.Empty(_detector.Detectar([partido]));
        Assert.NotEmpty(_detector.Detectar([partido], minimoCasas: 2));
    }

    [Fact]
    public void Detectar_Totales_UsaElConsensoDeSuLinea()
    {
        // Arrange
        var partido = ConTotales(new PartidoBuilder()).Build();

        // Act
        var desfasada = Assert.Single(_detector.Detectar([partido]));

        // Assert
        Assert.Equal(TipoMercado.Totales, desfasada.Mercado);
        Assert.Equal(2.5m, desfasada.Linea);
        Assert.Equal(ResultadoCuota.Mas, desfasada.Resultado);
        Assert.Equal("casaD", desfasada.Casa.Clave);
        Assert.Equal(0.493242m, desfasada.ProbabilidadJusta);
        Assert.Equal(2.027m, desfasada.CuotaJusta);
        Assert.Equal(3.58m, desfasada.VentajaPorcentaje);
    }

    [Fact]
    public void Detectar_Handicap_DevuelveLaLineaDeLaPropiaCuota()
    {
        // Arrange
        var partido = ConHandicap(new PartidoBuilder()).Build();

        // Act
        var desfasada = Assert.Single(_detector.Detectar([partido]));

        // Assert: el visitante se apuesta a +1.5 aunque el consenso se agrupe por la linea del local
        Assert.Equal(TipoMercado.Handicap, desfasada.Mercado);
        Assert.Equal(1.5m, desfasada.Linea);
        Assert.Equal(ResultadoCuota.Visitante, desfasada.Resultado);
        Assert.Equal(0.608949m, desfasada.ProbabilidadJusta);
        Assert.Equal(1.642m, desfasada.CuotaJusta);
        Assert.Equal(4.74m, desfasada.VentajaPorcentaje);
    }

    [Theory]
    [InlineData(TipoMercado.Ganador, 2)]
    [InlineData(TipoMercado.Totales, 1)]
    [InlineData(TipoMercado.Handicap, 1)]
    public void Detectar_ConMercado_SoloDevuelveCuotasDeEseMercado(TipoMercado mercado, int esperadas)
    {
        // Arrange
        var partido = ConHandicap(ConTotales(Partido1X2())).Build();

        // Act
        var desfasadas = _detector.Detectar([partido], mercado);

        // Assert
        Assert.Equal(esperadas, desfasadas.Count);
        Assert.All(desfasadas, d => Assert.Equal(mercado, d.Mercado));
    }

    [Fact]
    public void Detectar_SinMercado_UneTodosLosMercadosYPartidosPorVentaja()
    {
        // Arrange
        var partidos = new[]
        {
            ConTotales(new PartidoBuilder().ConId("partido-totales")).Build(),
            ConHandicap(Partido1X2()).Build()
        };

        // Act
        var desfasadas = _detector.Detectar(partidos);

        // Assert
        Assert.Equal([6.97m, 6.79m, 4.74m, 3.58m], desfasadas.Select(d => d.VentajaPorcentaje));
        Assert.Equal("partido-totales", desfasadas[^1].PartidoId);
    }

    [Fact]
    public void Detectar_SinPartidos_DevuelveListaVacia()
        => Assert.Empty(_detector.Detectar([]));

    [Fact]
    public void Detectar_VentajaMinimaNegativa_LanzaExcepcion()
        => Assert.Throws<ArgumentOutOfRangeException>(() => _detector.Detectar([], ventajaMinima: -1m));
}
