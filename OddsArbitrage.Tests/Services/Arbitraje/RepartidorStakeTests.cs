using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Tests.Builders;

namespace OddsArbitrage.Tests.Services.Arbitraje;

public class RepartidorStakeTests
{
    private const decimal ToleranciaRetorno = 0.02m;

    // Cuotas con arbitraje claro: suma de probabilidades implicitas = 0.992063... < 1
    private static readonly IReadOnlyList<Cuota> MejoresCuotasConArbitraje =
    [
        CuotaDePrueba.Crear(ResultadoPartido.Local, 2.10m),
        CuotaDePrueba.Crear(ResultadoPartido.Empate, 3.60m),
        CuotaDePrueba.Crear(ResultadoPartido.Visitante, 4.20m)
    ];

    [Fact]
    public void Calcular_ConCuotasDeArbitraje_LosRetornosDeTodasLasApuestasSonIguales()
    {
        // Arrange y Act
        var reparto = RepartidorStake.Calcular(MejoresCuotasConArbitraje, stakeTotal: 100m);

        // Assert
        var retornos = reparto.Apuestas.Select(a => a.Retorno).ToList();
        var maximo = retornos.Max();
        var minimo = retornos.Min();
        Assert.True(maximo - minimo <= ToleranciaRetorno,
            $"La diferencia entre retornos ({maximo - minimo}) supera la tolerancia de {ToleranciaRetorno}");
    }

    [Fact]
    public void Calcular_ConCuotasDeArbitraje_LaSumaDeStakesEsAproximadamenteElTotal()
    {
        // Arrange
        var stakeTotal = 100m;

        // Act
        var reparto = RepartidorStake.Calcular(MejoresCuotasConArbitraje, stakeTotal);

        // Assert
        var sumaStakes = reparto.Apuestas.Sum(a => a.Stake);
        Assert.True(Math.Abs(sumaStakes - stakeTotal) <= ToleranciaRetorno,
            $"La suma de stakes ({sumaStakes}) difiere demasiado del total ({stakeTotal})");
        Assert.Equal(sumaStakes, reparto.StakeTotal);
    }

    [Fact]
    public void Calcular_ConCuotasDeArbitraje_LaGananciaGarantizadaEsPositiva()
    {
        // Arrange y Act
        var reparto = RepartidorStake.Calcular(MejoresCuotasConArbitraje, stakeTotal: 100m);

        // Assert
        Assert.True(reparto.GananciaGarantizada > 0m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Calcular_StakeMenorOIgualACero_LanzaArgumentOutOfRangeException(decimal stakeTotal)
    {
        // Arrange, Act y Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RepartidorStake.Calcular(MejoresCuotasConArbitraje, stakeTotal));
    }
}
