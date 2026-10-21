using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.ValueBets;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.ValueBets;

public class ProbabilidadJustaTests
{
    // Tres casas en totales 2.5: sin margen dan 0.5, 0.526316 y 0.486842 para el mas
    private static PartidoBuilder TotalesConTresCasas() => new PartidoBuilder()
        .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.90m)
        .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.90m)
        .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.80m)
        .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.00m)
        .ConCuota("casaC", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.95m)
        .ConCuota("casaC", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.85m);

    [Fact]
    public void QuitarMargen_DosResultadosIguales_DaLaMitadACadaUno()
    {
        // Arrange: 1/1.90 + 1/1.90 = 1.052632, un margen de 5.26 %
        var cuotas = new[]
        {
            CuotaDePrueba.Crear(ResultadoCuota.Mas, 1.90m),
            CuotaDePrueba.Crear(ResultadoCuota.Menos, 1.90m)
        };

        // Act
        var probabilidades = ProbabilidadJusta.QuitarMargen(cuotas);

        // Assert
        Assert.Equal(0.5m, probabilidades[ResultadoCuota.Mas]);
        Assert.Equal(0.5m, probabilidades[ResultadoCuota.Menos]);
    }

    [Fact]
    public void QuitarMargen_1X2_RepartePorProporcionYSumaUno()
    {
        // Arrange: implicitas 0.5 + 0.285714 + 0.25 = 1.035714
        var cuotas = new[]
        {
            CuotaDePrueba.Crear(ResultadoCuota.Local, 2.00m),
            CuotaDePrueba.Crear(ResultadoCuota.Empate, 3.50m),
            CuotaDePrueba.Crear(ResultadoCuota.Visitante, 4.00m)
        };

        // Act
        var probabilidades = ProbabilidadJusta.QuitarMargen(cuotas);

        // Assert
        Assert.Equal(0.482759m, Math.Round(probabilidades[ResultadoCuota.Local], 6));
        Assert.Equal(0.275862m, Math.Round(probabilidades[ResultadoCuota.Empate], 6));
        Assert.Equal(0.241379m, Math.Round(probabilidades[ResultadoCuota.Visitante], 6));
        Assert.Equal(1m, Math.Round(probabilidades.Values.Sum(), 10));
    }

    [Fact]
    public void QuitarMargen_SinCuotas_LanzaExcepcion()
        => Assert.Throws<ArgumentException>(() => ProbabilidadJusta.QuitarMargen([]));

    [Fact]
    public void CalcularConsenso_TresCasas_PromediaLasProbabilidadesSinMargen()
    {
        // Arrange
        var partido = TotalesConTresCasas().Build();

        // Act
        var consenso = ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Totales, 2.5m);

        // Assert: (0.5 + 0.526316 + 0.486842) / 3 = 0.504386
        Assert.NotNull(consenso);
        Assert.Equal(3, consenso.Casas);
        Assert.Equal(2.5m, consenso.Linea);
        Assert.Equal(0.504386m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Mas], 6));
        Assert.Equal(0.495614m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Menos], 6));
        Assert.Equal(1m, Math.Round(consenso.ProbabilidadesJustas.Values.Sum(), 10));
    }

    [Fact]
    public void CalcularConsenso_1X2_SumaUnoYUsaTodosLosResultados()
    {
        // Arrange
        var partido = new PartidoBuilder()
            .ConCuota("casaA", ResultadoCuota.Local, 2.00m)
            .ConCuota("casaA", ResultadoCuota.Empate, 3.50m)
            .ConCuota("casaA", ResultadoCuota.Visitante, 4.00m)
            .ConCuota("casaB", ResultadoCuota.Local, 2.10m)
            .ConCuota("casaB", ResultadoCuota.Empate, 3.40m)
            .ConCuota("casaB", ResultadoCuota.Visitante, 3.60m)
            .ConCuota("casaC", ResultadoCuota.Local, 1.95m)
            .ConCuota("casaC", ResultadoCuota.Empate, 3.60m)
            .ConCuota("casaC", ResultadoCuota.Visitante, 4.20m)
            .Build();

        // Act
        var consenso = ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Ganador, linea: null);

        // Assert
        Assert.NotNull(consenso);
        Assert.Null(consenso.Linea);
        Assert.Equal(
            [ResultadoCuota.Local, ResultadoCuota.Empate, ResultadoCuota.Visitante],
            consenso.ProbabilidadesJustas.Keys);
        Assert.Equal(0.478539m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Local], 6));
        Assert.Equal(0.275505m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Empate], 6));
        Assert.Equal(0.245956m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Visitante], 6));
        Assert.Equal(1m, Math.Round(consenso.ProbabilidadesJustas.Values.Sum(), 10));
    }

    [Fact]
    public void CalcularConsenso_MenosCasasQueElMinimo_DevuelveNull()
    {
        // Arrange: solo dos casas y el minimo por defecto es 3
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.90m)
            .ConCuota("casaA", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 1.90m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 1.80m)
            .ConCuota("casaB", TipoMercado.Totales, 2.5m, ResultadoCuota.Menos, 2.00m)
            .Build();

        // Act y Assert
        Assert.Null(ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Totales, 2.5m));
        Assert.Equal(2, ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Totales, 2.5m, minimoCasas: 2)?.Casas);
    }

    [Fact]
    public void CalcularConsenso_CasaQueNoCotizaTodosLosResultados_NoCuenta()
    {
        // Arrange: casaD solo cotiza el mas, asi que no se le puede quitar el margen
        var partido = TotalesConTresCasas()
            .ConCuota("casaD", TipoMercado.Totales, 2.5m, ResultadoCuota.Mas, 3.00m)
            .Build();

        // Act
        var consenso = ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Totales, 2.5m);

        // Assert: el resultado es el mismo que con las tres casas completas
        Assert.NotNull(consenso);
        Assert.Equal(3, consenso.Casas);
        Assert.Equal(0.504386m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Mas], 6));
    }

    [Fact]
    public void CalcularConsenso_Handicap_EmparejaLaLineaDelLocalConLaOpuestaDelVisitante()
    {
        // Arrange: local -1.5 contra visitante +1.5 en tres casas
        var partido = new PartidoBuilder()
            .ConCuota("casaA", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.50m)
            .ConCuota("casaA", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.55m)
            .ConCuota("casaB", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.50m)
            .ConCuota("casaB", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.55m)
            .ConCuota("casaC", TipoMercado.Handicap, -1.5m, ResultadoCuota.Local, 2.50m)
            .ConCuota("casaC", TipoMercado.Handicap, 1.5m, ResultadoCuota.Visitante, 1.55m)
            .Build();

        // Act
        var consenso = ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Handicap, -1.5m);

        // Assert: 1/2.50 = 0.4 y 1/1.55 = 0.645161; sin margen 0.4 / 1.045161 = 0.382716
        Assert.NotNull(consenso);
        Assert.Equal(-1.5m, consenso.Linea);
        Assert.Equal(0.382716m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Local], 6));
        Assert.Equal(0.617284m, Math.Round(consenso.ProbabilidadesJustas[ResultadoCuota.Visitante], 6));
        Assert.Null(ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Handicap, 1.5m));
    }

    [Fact]
    public void CalcularConsensos_SeparaMercadosYLineas()
    {
        // Arrange: totales 2.5 con tres casas, totales 3.5 con una sola y 1X2 sin cuotas
        var partido = TotalesConTresCasas()
            .ConCuota("casaA", TipoMercado.Totales, 3.5m, ResultadoCuota.Mas, 2.80m)
            .ConCuota("casaA", TipoMercado.Totales, 3.5m, ResultadoCuota.Menos, 1.40m)
            .Build();

        // Act
        var consensos = ProbabilidadJusta.CalcularConsensos(partido);
        var conMinimoUno = ProbabilidadJusta.CalcularConsensos(partido, minimoCasas: 1);

        // Assert
        var consenso = Assert.Single(consensos);
        Assert.Equal((TipoMercado.Totales, 2.5m), (consenso.Mercado, consenso.Linea!.Value));
        Assert.Equal([2.5m, 3.5m], conMinimoUno.Select(c => c.Linea!.Value));
    }

    [Fact]
    public void CalcularConsenso_MinimoDeCasasMenorAUno_LanzaExcepcion()
    {
        // Arrange
        var partido = TotalesConTresCasas().Build();

        // Act y Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ProbabilidadJusta.CalcularConsenso(partido, TipoMercado.Totales, 2.5m, minimoCasas: 0));
    }
}
