using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

// Contrato comun de los detectores de surebets; cada implementacion cubre uno o mas mercados
public interface IDetectorSurebets
{
    const decimal StakePorDefecto = 100m;

    IReadOnlyCollection<TipoMercado> Mercados { get; }

    // Evalua varios partidos y devuelve solo las surebets de sus mercados, ordenadas por margen descendente
    IReadOnlyList<OportunidadArbitraje> Detectar(IEnumerable<Partido> partidos, decimal stakeTotal = StakePorDefecto);
}
