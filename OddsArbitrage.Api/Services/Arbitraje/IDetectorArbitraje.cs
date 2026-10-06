using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

public interface IDetectorArbitraje
{
    // Devuelve la oportunidad si el partido tiene arbitraje en el mercado 1X2, o null si no lo tiene
    OportunidadArbitraje? Evaluar(Partido partido, decimal stakeTotal = DetectorArbitraje.StakePorDefecto);

    // Evalua varios partidos y devuelve solo los que presentan arbitraje, ordenados por margen descendente
    IReadOnlyList<OportunidadArbitraje> Detectar(IEnumerable<Partido> partidos, decimal stakeTotal = DetectorArbitraje.StakePorDefecto);
}
