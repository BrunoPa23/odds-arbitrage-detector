using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.Arbitraje;

public class DetectorDosResultadosHandicapTests
{
    private readonly DetectorDosResultados _detector = new();

    [Fact]
    public void Evaluar_HandicapLocalNegativoConVisitantePositivo_DetectaSurebet()
    {
        // Arrange: local -1.5 y visitante +1.5 son complementarios; 1/2.60 + 1/1.70 = 0.972851 < 1
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.60m)
            .ConCuota("casaA", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.50m)
            .ConCuota("casaB", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.40m)
            .ConCuota("casaB", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.70m)
            .Build();

        // Act
        var surebet = Assert.Single(_detector.Evaluar(partido));

        // Assert: la linea de la oportunidad es la del local
        Assert.Equal(TipoMercado.Handicap, surebet.Mercado);
        Assert.Equal(-1.5m, surebet.Linea);
        Assert.Equal(0.972851m, surebet.SumaProbabilidadesImplicitas);
        Assert.Equal(2.79m, surebet.MargenPorcentaje);

        Assert.Collection(surebet.MejoresCuotas,
            local =>
            {
                Assert.Equal(ResultadoCuota.Local, local.Resultado);
                Assert.Equal("casaA", local.Casa.Clave);
                Assert.Equal(-1.5m, local.Linea);
            },
            visitante =>
            {
                Assert.Equal(ResultadoCuota.Visitante, visitante.Resultado);
                Assert.Equal("casaB", visitante.Casa.Clave);
                Assert.Equal(1.5m, visitante.Linea);
            });
    }

    [Fact]
    public void Evaluar_HandicapConLaMismaLineaParaAmbosEquipos_NoLosEmpareja()
    {
        // Arrange: local -1.5 y visitante -1.5 no son complementarios (un empate pierde ambas)
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.60m)
            .ConCuota("casaB", TipoMercado.Handicap, -1.5m, ResultadoCuota.Visitante, 2.60m)
            .Build();

        // Act y Assert
        Assert.Empty(_detector.Evaluar(partido));
    }

    [Fact]
    public void Evaluar_HandicapConLineaPositivaDelLocal_EmparejaConVisitanteNegativo()
    {
        // Arrange: el local recibe +0.5 y el visitante da -0.5
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, 0.5m, ResultadoCuota.Local, 2.10m)
            .ConCuota("casaB", TipoMercado.Handicap, -0.5m, ResultadoCuota.Visitante, 2.05m)
            .Build();

        // Act
        var surebet = Assert.Single(_detector.Evaluar(partido));

        // Assert
        Assert.Equal(0.5m, surebet.Linea);
        Assert.Equal(3.73m, surebet.MargenPorcentaje);
    }

    [Fact]
    public void Evaluar_HandicapConLineaCero_EmparejaAmbosEquipos()
    {
        // Arrange: handicap 0 (empate devuelve la apuesta); el visitante tambien viene con linea 0
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, 0m, ResultadoCuota.Local, 2.10m)
            .ConCuota("casaB", TipoMercado.Handicap, 0m, ResultadoCuota.Visitante, 2.05m)
            .Build();

        // Act
        var surebet = Assert.Single(_detector.Evaluar(partido));

        // Assert
        Assert.Equal(0m, surebet.Linea);
        Assert.Equal(2, surebet.MejoresCuotas.Count);
    }

    [Fact]
    public void Evaluar_HandicapConVariasLineas_SoloDevuelveLasQueTienenSurebet()
    {
        // Arrange: la linea -0.5 tiene surebet y la -1.5 no
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, -0.5m, ResultadoCuota.Local, 2.10m)
            .ConCuota("casaB", TipoMercado.Handicap, 0.5m, ResultadoCuota.Visitante, 2.05m)
            .ConCuota("casaA", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 3.00m)
            .ConCuota("casaB", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.40m)
            .Build();

        // Act
        var surebet = Assert.Single(_detector.Evaluar(partido));

        // Assert
        Assert.Equal(-0.5m, surebet.Linea);
    }

    [Fact]
    public void Evaluar_HandicapYTotalesEnElMismoPartido_DevuelveUnaSurebetPorMercado()
    {
        // Arrange: misma cifra de linea en ambos mercados para verificar que no se mezclan
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, 2.5m, ResultadoCuota.Local, 2.10m)
            .ConCuota("casaB", TipoMercado.Handicap, -2.5m, ResultadoCuota.Visitante, 2.05m)
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 2.10m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.05m)
            .Build();

        // Act
        var surebets = _detector.Evaluar(partido);

        // Assert
        Assert.Equal(2, surebets.Count);
        Assert.Contains(surebets, s => s.Mercado == TipoMercado.Totales && s.Linea == 2.5m);
        Assert.Contains(surebets, s => s.Mercado == TipoMercado.Handicap && s.Linea == 2.5m);
        Assert.All(surebets, s => Assert.All(s.MejoresCuotas, c => Assert.Equal(s.Mercado, c.Mercado)));
    }

    [Fact]
    public void Evaluar_HandicapIgnoraElEmpate()
    {
        // Arrange: una cuota de empate en handicap no es un resultado valido y no completa el mercado
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, -0.5m, ResultadoCuota.Local, 2.10m)
            .ConCuota("casaB", TipoMercado.Handicap, -0.5m, ResultadoCuota.Empate, 9.00m)
            .Build();

        // Act y Assert
        Assert.Empty(_detector.Evaluar(partido));
    }
}
