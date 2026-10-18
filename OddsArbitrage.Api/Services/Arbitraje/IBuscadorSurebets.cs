using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

public interface IBuscadorSurebets
{
    // Corre los detectores del mercado pedido (o todos si es null) y une sus surebets por margen descendente
    IReadOnlyList<OportunidadArbitraje> Buscar(
        IEnumerable<Partido> partidos,
        TipoMercado? mercado = null,
        decimal stakeTotal = IDetectorSurebets.StakePorDefecto);
}
