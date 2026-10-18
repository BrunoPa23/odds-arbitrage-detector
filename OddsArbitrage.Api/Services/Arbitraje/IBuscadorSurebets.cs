using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

public interface IBuscadorSurebets
{
    // Corre los detectores del mercado pedido (o todos si es null) y une sus surebets por margen descendente.
    // Con margenMinimo solo quedan las surebets cuyo margen porcentual es mayor o igual a ese valor
    IReadOnlyList<OportunidadArbitraje> Buscar(
        IEnumerable<Partido> partidos,
        TipoMercado? mercado = null,
        decimal stakeTotal = IDetectorSurebets.StakePorDefecto,
        decimal? margenMinimo = null);
}
