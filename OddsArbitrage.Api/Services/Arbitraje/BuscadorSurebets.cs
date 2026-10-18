using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

// Punto unico de entrada: no conoce los detectores concretos, solo los registrados en DI
public sealed class BuscadorSurebets(IEnumerable<IDetectorSurebets> detectores) : IBuscadorSurebets
{
    private readonly IReadOnlyList<IDetectorSurebets> _detectores = detectores.ToList();

    public IReadOnlyList<OportunidadArbitraje> Buscar(
        IEnumerable<Partido> partidos,
        TipoMercado? mercado = null,
        decimal stakeTotal = IDetectorSurebets.StakePorDefecto,
        decimal? margenMinimo = null)
    {
        var listaPartidos = partidos as IReadOnlyCollection<Partido> ?? partidos.ToList();

        return _detectores
            .Where(d => mercado is null || d.Mercados.Contains(mercado.Value))
            .SelectMany(d => d.Detectar(listaPartidos, stakeTotal))
            // Un detector puede cubrir varios mercados, por eso se filtra tambien cada surebet
            .Where(o => mercado is null || o.Mercado == mercado)
            .Where(o => margenMinimo is null || o.MargenPorcentaje >= margenMinimo)
            .OrderByDescending(o => o.MargenPorcentaje)
            .ToList();
    }
}
