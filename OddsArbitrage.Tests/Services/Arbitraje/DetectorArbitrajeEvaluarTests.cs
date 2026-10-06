using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.Arbitraje;

public class DetectorArbitrajeEvaluarTests
{
    private readonly DetectorArbitraje _detector = new();

    [Fact]
    public void Evaluar_ConCuotasDeDosCasas_DetectaArbitraje()
    {
        // Arrange
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaA", ResultadoPartido.Empate, 4.00m)
            .ConCuota("casaB", ResultadoPartido.Visitante, 5.00m)
            .Build();

        // Act
        var oportunidad = _detector.Evaluar(partido);

        // Assert
        Assert.NotNull(oportunidad);
        Assert.Equal(0.95m, oportunidad!.SumaProbabilidadesImplicitas);
    }

    [Fact]
    public void Evaluar_SumaDeProbabilidadesMayorOIgualAUno_DevuelveNull()
    {
        // Arrange
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoPartido.Local, 1.50m)
            .ConCuota("casaA", ResultadoPartido.Empate, 3.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 3.00m)
            .Build();

        // Act
        var oportunidad = _detector.Evaluar(partido);

        // Assert
        Assert.Null(oportunidad);
    }

    [Fact]
    public void Evaluar_FaltaUnResultadoDelMercado_DevuelveNull()
    {
        // Arrange: no hay ninguna cuota para Visitante
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaA", ResultadoPartido.Empate, 4.00m)
            .Build();

        // Act
        var oportunidad = _detector.Evaluar(partido);

        // Assert
        Assert.Null(oportunidad);
    }

    [Fact]
    public void Evaluar_ConVariasCasas_EligeLaCuotaMasAltaPorResultado()
    {
        // Arrange
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaB", ResultadoPartido.Local, 2.50m)
            .ConCuota("casaA", ResultadoPartido.Empate, 4.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 5.00m)
            .Build();

        // Act
        var oportunidad = _detector.Evaluar(partido);

        // Assert
        Assert.NotNull(oportunidad);
        var cuotaLocal = oportunidad!.MejoresCuotas.Single(c => c.Resultado == ResultadoPartido.Local);
        Assert.Equal(2.50m, cuotaLocal.Valor);
        Assert.Equal("casaB", cuotaLocal.Casa.Clave);
    }

    [Fact]
    public void Evaluar_IgnoraCuotasMenoresOIgualesAUno()
    {
        // Arrange: la cuota de 1.00 para Local no es valida y debe ser ignorada
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoPartido.Local, 1.00m)
            .ConCuota("casaB", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaA", ResultadoPartido.Empate, 4.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 5.00m)
            .Build();

        // Act
        var oportunidad = _detector.Evaluar(partido);

        // Assert
        Assert.NotNull(oportunidad);
        var cuotaLocal = oportunidad!.MejoresCuotas.Single(c => c.Resultado == ResultadoPartido.Local);
        Assert.Equal(2.00m, cuotaLocal.Valor);
        Assert.Equal("casaB", cuotaLocal.Casa.Clave);
    }

    [Fact]
    public void Evaluar_MargenCalculadoCorrectamente_ConRedondeoADosDecimales()
    {
        // Arrange: suma de probabilidades = 0.95, margen esperado = (1/0.95 - 1) x 100 = 5.26...
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaA", ResultadoPartido.Empate, 4.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 5.00m)
            .Build();

        // Act
        var oportunidad = _detector.Evaluar(partido);

        // Assert
        Assert.NotNull(oportunidad);
        Assert.Equal(5.26m, oportunidad!.MargenPorcentaje);
    }
}
