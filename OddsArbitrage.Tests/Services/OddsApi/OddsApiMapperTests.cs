using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Tests.Services.OddsApi;

public class OddsApiMapperTests
{
    private static readonly DateTimeOffset FechaInicio = new(2026, 3, 15, 18, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UltimaActualizacion = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void APartido_ConDtoDeDosCasas_MapeaPartidoYCuotasCorrectamente()
    {
        // Arrange
        var evento = new EventDto(
            Id: "evento-1",
            SportKey: "soccer_epl",
            SportTitle: "Premier League",
            CommenceTime: FechaInicio,
            HomeTeam: "Equipo Local",
            AwayTeam: "Equipo Visitante",
            Bookmakers:
            [
                new BookmakerDto(
                    Key: "casaA",
                    Title: "Casa A",
                    LastUpdate: UltimaActualizacion,
                    Markets:
                    [
                        new MarketDto(
                            Key: OddsApiMapper.MercadoH2H,
                            LastUpdate: null,
                            Outcomes:
                            [
                                new OutcomeDto("Equipo Local", 2.10m),
                                new OutcomeDto("Draw", 3.40m),
                                new OutcomeDto("Equipo Visitante", 3.80m)
                            ])
                    ]),
                new BookmakerDto(
                    Key: "casaB",
                    Title: "Casa B",
                    LastUpdate: UltimaActualizacion,
                    Markets:
                    [
                        new MarketDto(
                            Key: OddsApiMapper.MercadoH2H,
                            LastUpdate: null,
                            Outcomes:
                            [
                                new OutcomeDto("Equipo Local", 2.05m),
                                new OutcomeDto("Draw", 3.50m),
                                new OutcomeDto("Equipo Visitante", 3.90m)
                            ])
                    ])
            ]);

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Equal("evento-1", partido.Id);
        Assert.Equal("Premier League", partido.Liga);
        Assert.Equal("Equipo Local", partido.EquipoLocal);
        Assert.Equal("Equipo Visitante", partido.EquipoVisitante);
        Assert.Equal(FechaInicio, partido.FechaInicio);
        Assert.Equal(6, partido.Cuotas.Count);

        var cuotaLocalCasaA = Assert.Single(partido.Cuotas,
            c => c.Casa.Clave == "casaA" && c.Resultado == ResultadoPartido.Local);
        Assert.Equal(2.10m, cuotaLocalCasaA.Valor);
        Assert.Equal("Casa A", cuotaLocalCasaA.Casa.Nombre);

        var cuotaEmpateCasaB = Assert.Single(partido.Cuotas,
            c => c.Casa.Clave == "casaB" && c.Resultado == ResultadoPartido.Empate);
        Assert.Equal(3.50m, cuotaEmpateCasaB.Valor);

        var cuotaVisitanteCasaA = Assert.Single(partido.Cuotas,
            c => c.Casa.Clave == "casaA" && c.Resultado == ResultadoPartido.Visitante);
        Assert.Equal(3.80m, cuotaVisitanteCasaA.Valor);
    }

    [Fact]
    public void APartido_IgnoraMercadosQueNoSonH2H()
    {
        // Arrange
        var evento = new EventDto(
            Id: "evento-2",
            SportKey: "soccer_epl",
            SportTitle: "Premier League",
            CommenceTime: FechaInicio,
            HomeTeam: "Equipo Local",
            AwayTeam: "Equipo Visitante",
            Bookmakers:
            [
                new BookmakerDto(
                    Key: "casaA",
                    Title: "Casa A",
                    LastUpdate: UltimaActualizacion,
                    Markets:
                    [
                        new MarketDto(
                            Key: "totals",
                            LastUpdate: null,
                            Outcomes: [new OutcomeDto("Over", 1.90m)])
                    ])
            ]);

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Empty(partido.Cuotas);
    }

    [Fact]
    public void APartido_IgnoraOutcomesConNombreNoReconocido()
    {
        // Arrange: el nombre "Empate" no coincide con ningun equipo ni con "Draw"
        var evento = new EventDto(
            Id: "evento-3",
            SportKey: "soccer_epl",
            SportTitle: "Premier League",
            CommenceTime: FechaInicio,
            HomeTeam: "Equipo Local",
            AwayTeam: "Equipo Visitante",
            Bookmakers:
            [
                new BookmakerDto(
                    Key: "casaA",
                    Title: "Casa A",
                    LastUpdate: UltimaActualizacion,
                    Markets:
                    [
                        new MarketDto(
                            Key: OddsApiMapper.MercadoH2H,
                            LastUpdate: null,
                            Outcomes:
                            [
                                new OutcomeDto("Equipo Local", 2.10m),
                                new OutcomeDto("Empate", 3.40m)
                            ])
                    ])
            ]);

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Single(partido.Cuotas);
        Assert.Equal(ResultadoPartido.Local, partido.Cuotas[0].Resultado);
    }

    [Fact]
    public void APartido_UsaUltimaActualizacionDelMercado_CuandoEstaPresente()
    {
        // Arrange
        var actualizacionMercado = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var evento = new EventDto(
            Id: "evento-4",
            SportKey: "soccer_epl",
            SportTitle: "Premier League",
            CommenceTime: FechaInicio,
            HomeTeam: "Equipo Local",
            AwayTeam: "Equipo Visitante",
            Bookmakers:
            [
                new BookmakerDto(
                    Key: "casaA",
                    Title: "Casa A",
                    LastUpdate: UltimaActualizacion,
                    Markets:
                    [
                        new MarketDto(
                            Key: OddsApiMapper.MercadoH2H,
                            LastUpdate: actualizacionMercado,
                            Outcomes: [new OutcomeDto("Equipo Local", 2.10m)])
                    ])
            ]);

        // Act
        var partido = OddsApiMapper.APartido(evento);

        // Assert
        Assert.Equal(actualizacionMercado, partido.Cuotas[0].UltimaActualizacion);
    }
}
