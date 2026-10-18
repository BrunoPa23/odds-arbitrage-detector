using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Tests.Services.OddsApi;

// Tests del mapper para los mercados de dos resultados (totals y spreads), con DTOs ficticios
public class OddsApiMapperMercadosTests
{
    private static readonly DateTimeOffset FechaInicio = new(2026, 3, 15, 18, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UltimaActualizacion = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void APartido_ConTotals_MapeaOverYUnderConSuLinea()
    {
        // Arrange
        var evento = CrearEvento(new MarketDto(MercadosOddsApi.Totals, null,
        [
            new OutcomeDto("Over", 1.95m, 2.5m),
            new OutcomeDto("Under", 1.90m, 2.5m)
        ]));

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Equal(2, partido.Cuotas.Count);
        Assert.All(partido.Cuotas, c =>
        {
            Assert.Equal(TipoMercado.Totales, c.Mercado);
            Assert.Equal(2.5m, c.Linea);
        });
        Assert.Equal(1.95m, Assert.Single(partido.Cuotas, c => c.Resultado == ResultadoCuota.Mas).Valor);
        Assert.Equal(1.90m, Assert.Single(partido.Cuotas, c => c.Resultado == ResultadoCuota.Menos).Valor);
    }

    [Fact]
    public void APartido_ConTotalsDeVariasLineas_ConservaCadaLinea()
    {
        // Arrange
        var evento = CrearEvento(new MarketDto(MercadosOddsApi.Totals, null,
        [
            new OutcomeDto("Over", 1.50m, 1.5m),
            new OutcomeDto("Under", 2.60m, 1.5m),
            new OutcomeDto("Over", 2.40m, 3.5m),
            new OutcomeDto("Under", 1.55m, 3.5m)
        ]));

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Equal([1.5m, 3.5m], partido.Cuotas.Select(c => c.Linea!.Value).Distinct().Order());
        Assert.Equal(2.40m, Assert.Single(partido.Cuotas, c => c.Resultado == ResultadoCuota.Mas && c.Linea == 3.5m).Valor);
    }

    [Fact]
    public void APartido_ConSpreads_MapeaCadaEquipoConSuHandicap()
    {
        // Arrange: el local da 1.5 goles de ventaja y el visitante los recibe
        var evento = CrearEvento(new MarketDto(MercadosOddsApi.Spreads, null,
        [
            new OutcomeDto("Equipo Local", 2.30m, -1.5m),
            new OutcomeDto("Equipo Visitante", 1.65m, 1.5m)
        ]));

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.All(partido.Cuotas, c => Assert.Equal(TipoMercado.Handicap, c.Mercado));

        var local = Assert.Single(partido.Cuotas, c => c.Resultado == ResultadoCuota.Local);
        Assert.Equal(-1.5m, local.Linea);
        Assert.Equal(2.30m, local.Valor);

        var visitante = Assert.Single(partido.Cuotas, c => c.Resultado == ResultadoCuota.Visitante);
        Assert.Equal(1.5m, visitante.Linea);
        Assert.Equal(1.65m, visitante.Valor);
    }

    [Fact]
    public void APartido_ConSpreadsYDraw_IgnoraElEmpate()
    {
        // Arrange: en handicap no existe el empate como resultado
        var evento = CrearEvento(new MarketDto(MercadosOddsApi.Spreads, null,
        [
            new OutcomeDto("Equipo Local", 1.90m, -0.5m),
            new OutcomeDto("Draw", 3.40m, 0m)
        ]));

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Equal(ResultadoCuota.Local, Assert.Single(partido.Cuotas).Resultado);
    }

    [Theory]
    [InlineData(MercadosOddsApi.Totals, "Over")]
    [InlineData(MercadosOddsApi.Spreads, "Equipo Local")]
    public void APartido_OutcomeSinPoint_SeIgnora(string clave, string nombre)
    {
        // Arrange
        var evento = CrearEvento(new MarketDto(clave, null, [new OutcomeDto(nombre, 1.90m)]));

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Empty(partido.Cuotas);
    }

    [Fact]
    public void APartido_EnTotals_IgnoraNombresQueNoSonOverNiUnder()
    {
        // Arrange: un nombre de equipo no tiene sentido en totales
        var evento = CrearEvento(new MarketDto(MercadosOddsApi.Totals, null,
        [
            new OutcomeDto("Equipo Local", 1.90m, 2.5m),
            new OutcomeDto("Under", 1.90m, 2.5m)
        ]));

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Equal(ResultadoCuota.Menos, Assert.Single(partido.Cuotas).Resultado);
    }

    [Fact]
    public void APartido_ConH2HYOtrosMercados_MapeaTodosSinLineaEnH2H()
    {
        // Arrange
        var evento = CrearEvento(
            new MarketDto(MercadosOddsApi.H2H, null,
            [
                new OutcomeDto("Equipo Local", 2.10m),
                new OutcomeDto("Draw", 3.40m),
                new OutcomeDto("Equipo Visitante", 3.80m)
            ]),
            new MarketDto(MercadosOddsApi.Totals, null,
            [
                new OutcomeDto("Over", 1.95m, 2.5m),
                new OutcomeDto("Under", 1.90m, 2.5m)
            ]),
            new MarketDto(MercadosOddsApi.Spreads, null,
            [
                new OutcomeDto("Equipo Local", 1.85m, -0.5m),
                new OutcomeDto("Equipo Visitante", 2.00m, 0.5m)
            ]));

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Equal(7, partido.Cuotas.Count);
        Assert.Equal(3, partido.Cuotas.Count(c => c.Mercado == TipoMercado.Ganador && c.Linea is null));
        Assert.Equal(2, partido.Cuotas.Count(c => c.Mercado == TipoMercado.Totales));
        Assert.Equal(2, partido.Cuotas.Count(c => c.Mercado == TipoMercado.Handicap));
    }

    private static EventDto CrearEvento(params MarketDto[] mercados) => new(
        Id: "evento-mercados",
        SportKey: "soccer_epl",
        SportTitle: "Premier League",
        CommenceTime: FechaInicio,
        HomeTeam: "Equipo Local",
        AwayTeam: "Equipo Visitante",
        Bookmakers: [new BookmakerDto("casaA", "Casa A", UltimaActualizacion, mercados)]);
}
