using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.Arbitraje;

public class DetectorDosResultadosTotalesTests
{
    private const decimal ToleranciaRetorno = 0.02m;

    private readonly DetectorDosResultados _detector = new();

    [Fact]
    public void Evaluar_TotalesConMejoresCuotasDeDosCasas_DetectaSurebet()
    {
        // Arrange: 1/2.10 + 1/2.05 = 0.963995 < 1
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.10m)
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.80m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.85m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.05m)
            .Build();

        // Act
        var surebet = Assert.Single(_detector.Evaluar(partido));

        // Assert
        Assert.Equal(TipoMercado.Totales, surebet.Mercado);
        Assert.Equal(2.5m, surebet.Linea);
        Assert.Equal(0.963995m, surebet.SumaProbabilidadesImplicitas);
        Assert.Equal(3.73m, surebet.MargenPorcentaje);

        Assert.Collection(surebet.MejoresCuotas,
            mas =>
            {
                Assert.Equal(ResultadoCuota.Mas, mas.Resultado);
                Assert.Equal("casaA", mas.Casa.Clave);
                Assert.Equal(2.10m, mas.Valor);
            },
            menos =>
            {
                Assert.Equal(ResultadoCuota.Menos, menos.Resultado);
                Assert.Equal("casaB", menos.Casa.Clave);
                Assert.Equal(2.05m, menos.Valor);
            });
    }

    [Fact]
    public void Evaluar_TotalesConMargenDeLaCasa_NoDetectaSurebet()
    {
        // Arrange: 1/1.90 + 1/1.90 = 1.0526, la casa se queda con el margen
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.90m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.90m)
            .Build();

        // Act y Assert
        Assert.Empty(_detector.Evaluar(partido));
    }

    [Fact]
    public void Evaluar_TotalesDeLineasDistintas_NoSeMezclan()
    {
        // Arrange: mas de 2.5 y menos de 3.5 no cubren todos los resultados (exactamente 3 goles pierde ambas)
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.20m)
            .ConCuota("casaB", TipoMercado.Totales, 3.5m, ResultadoCuota.Menos, 2.20m)
            .Build();

        // Act y Assert
        Assert.Empty(_detector.Evaluar(partido));
    }

    [Fact]
    public void Evaluar_TotalesConVariasLineasConSurebet_DevuelveUnaPorLinea()
    {
        // Arrange
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Totales, 3.5m, ResultadoCuota.Mas, 2.05m)
            .ConCuota("casaB", TipoMercado.Totales, 3.5m, ResultadoCuota.Menos, 2.02m)
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.20m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.95m)
            .Build();

        // Act
        var surebets = _detector.Evaluar(partido);

        // Assert: dentro de un partido quedan ordenadas por linea
        Assert.Equal(2, surebets.Count);
        Assert.Equal(2.5m, surebets[0].Linea);
        Assert.Equal(3.37m, surebets[0].MargenPorcentaje);
        Assert.Equal(3.5m, surebets[1].Linea);
        Assert.Equal(1.74m, surebets[1].MargenPorcentaje);
    }

    [Fact]
    public void Evaluar_TotalesSinUnoDeLosLados_NoDetectaSurebet()
    {
        // Arrange: solo hay cuotas a mas de 2.5
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 5.00m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 4.00m)
            .Build();

        // Act y Assert
        Assert.Empty(_detector.Evaluar(partido));
    }

    [Fact]
    public void Evaluar_TotalesConSurebet_RepartoIgualaLosRetornos()
    {
        // Arrange
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.10m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.05m)
            .Build();

        // Act
        var surebet = Assert.Single(_detector.Evaluar(partido, stakeTotal: 200m));

        // Assert
        var retornos = surebet.Reparto.Apuestas.Select(a => a.Retorno).ToList();
        Assert.Equal(2, retornos.Count);
        Assert.True(retornos.Max() - retornos.Min() <= ToleranciaRetorno);
        Assert.True(Math.Abs(surebet.Reparto.StakeTotal - 200m) <= ToleranciaRetorno);
        Assert.True(surebet.Reparto.GananciaGarantizada > 0m);
    }

    [Fact]
    public void Evaluar_IgnoraCuotasDelMercadoGanadorYCuotasInvalidas()
    {
        // Arrange: el 1X2 tiene arbitraje, pero no es asunto de este detector; la cuota 1.00 no es valida
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoCuota.Local, 2.00m)
            .ConCuota("casaA", ResultadoCuota.Empate, 4.00m)
            .ConCuota("casaA", ResultadoCuota.Visitante, 5.00m)
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.00m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 3.00m)
            .Build();

        // Act y Assert
        Assert.Empty(_detector.Evaluar(partido));
    }

    [Fact]
    public void Detectar_VariosPartidos_OrdenaPorMargenDescendente()
    {
        // Arrange
        var margenBajo = new PartidoBuilder()
            .ConId("margen-bajo")
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.05m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.02m)
            .Build();

        var margenAlto = new PartidoBuilder()
            .ConId("margen-alto")
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.10m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.05m)
            .Build();

        var sinSurebet = new PartidoBuilder()
            .ConId("sin-surebet")
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.90m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.90m)
            .Build();

        // Act
        var surebets = _detector.Detectar([margenBajo, sinSurebet, margenAlto]);

        // Assert
        Assert.Equal(["margen-alto", "margen-bajo"], surebets.Select(s => s.PartidoId));
    }
}
