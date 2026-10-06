using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.Arbitraje;

public class DetectorArbitrajeDetectarTests
{
    private readonly DetectorArbitraje _detector = new();

    [Fact]
    public void Detectar_FiltraPartidosSinArbitraje()
    {
        // Arrange
        var partidoConArbitraje = new PartidoBuilder()
            .ConId("con-arbitraje")
            .ConCuota("casaA", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaA", ResultadoPartido.Empate, 4.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 5.00m)
            .Build();

        var partidoSinArbitraje = new PartidoBuilder()
            .ConId("sin-arbitraje")
            .ConCuota("casaA", ResultadoPartido.Local, 1.50m)
            .ConCuota("casaA", ResultadoPartido.Empate, 3.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 3.00m)
            .Build();

        // Act
        var oportunidades = _detector.Detectar([partidoConArbitraje, partidoSinArbitraje]);

        // Assert
        Assert.Single(oportunidades);
        Assert.Equal("con-arbitraje", oportunidades[0].PartidoId);
    }

    [Fact]
    public void Detectar_VariosPartidosConArbitraje_OrdenaPorMargenDescendente()
    {
        // Arrange
        var partidoMargenBajo = new PartidoBuilder()
            .ConId("margen-bajo")
            .ConCuota("casaA", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaA", ResultadoPartido.Empate, 3.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 7.00m)
            .Build();

        var partidoMargenAlto = new PartidoBuilder()
            .ConId("margen-alto")
            .ConCuota("casaA", ResultadoPartido.Local, 2.00m)
            .ConCuota("casaA", ResultadoPartido.Empate, 4.00m)
            .ConCuota("casaA", ResultadoPartido.Visitante, 5.00m)
            .Build();

        // Act: se pasan en orden inverso al esperado para verificar que Detectar realmente ordena
        var oportunidades = _detector.Detectar([partidoMargenBajo, partidoMargenAlto]);

        // Assert
        Assert.Equal(2, oportunidades.Count);
        Assert.Equal("margen-alto", oportunidades[0].PartidoId);
        Assert.Equal("margen-bajo", oportunidades[1].PartidoId);
        Assert.True(oportunidades[0].MargenPorcentaje > oportunidades[1].MargenPorcentaje);
    }

    [Fact]
    public void Detectar_SinPartidos_DevuelveListaVacia()
    {
        // Arrange
        var partidos = Array.Empty<Partido>();

        // Act
        var oportunidades = _detector.Detectar(partidos);

        // Assert
        Assert.Empty(oportunidades);
    }
}
