using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.Arbitraje;

public class ProbabilidadImplicitaTests
{
    [Fact]
    public void Calcular_CuotaDosPuntoCero_DevuelveCincuentaPorCiento()
    {
        // Arrange
        var cuota = 2.00m;

        // Act
        var probabilidad = ProbabilidadImplicita.Calcular(cuota);

        // Assert
        Assert.Equal(0.5m, probabilidad);
    }

    [Fact]
    public void Calcular_CuotaCuatroPuntoCero_DevuelveVeinticincoPorCiento()
    {
        // Arrange
        var cuota = 4.00m;

        // Act
        var probabilidad = ProbabilidadImplicita.Calcular(cuota);

        // Assert
        Assert.Equal(0.25m, probabilidad);
    }

    [Theory]
    [InlineData(1.00)]
    [InlineData(0.50)]
    [InlineData(0)]
    [InlineData(-1.00)]
    public void Calcular_CuotaMenorOIgualAUno_LanzaArgumentOutOfRangeException(decimal cuota)
    {
        // Arrange, Act y Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ProbabilidadImplicita.Calcular(cuota));
    }

    [Fact]
    public void CalcularSuma_VariasCuotas_DevuelveSumaDeProbabilidadesIndividuales()
    {
        // Arrange
        var cuotas = new[]
        {
            CuotaDePrueba.Crear(ResultadoPartido.Local, 2.00m),
            CuotaDePrueba.Crear(ResultadoPartido.Empate, 4.00m),
            CuotaDePrueba.Crear(ResultadoPartido.Visitante, 4.00m)
        };

        // Act
        var suma = ProbabilidadImplicita.CalcularSuma(cuotas);

        // Assert
        Assert.Equal(1.0m, suma);
    }
}
