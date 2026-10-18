using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

public sealed class DetectorArbitraje : IDetectorArbitraje
{
    public const decimal StakePorDefecto = 100m;

    private static readonly IReadOnlyList<ResultadoCuota> Resultados1X2 = ResultadosPorMercado.De(TipoMercado.Ganador);

    public OportunidadArbitraje? Evaluar(Partido partido, decimal stakeTotal = StakePorDefecto)
    {
        var mejoresCuotas = SeleccionarMejoresCuotas(partido.Cuotas);

        // Si falta algun resultado no se puede cubrir el mercado completo, por lo tanto no hay arbitraje
        if (mejoresCuotas.Count != Resultados1X2.Count)
        {
            return null;
        }

        return ConstructorOportunidad.Crear(partido, TipoMercado.Ganador, linea: null, mejoresCuotas, stakeTotal);
    }

    public IReadOnlyList<OportunidadArbitraje> Detectar(IEnumerable<Partido> partidos, decimal stakeTotal = StakePorDefecto)
        => partidos
            .Select(p => Evaluar(p, stakeTotal))
            .OfType<OportunidadArbitraje>()
            .OrderByDescending(o => o.MargenPorcentaje)
            .ToList();

    // Para cada resultado (Local, Empate, Visitante) toma la cuota mas alta entre todas las casas.
    // Solo cuenta el mercado ganador; totales y handicap tienen su propia logica
    private static List<Cuota> SeleccionarMejoresCuotas(IEnumerable<Cuota> cuotas)
        => cuotas
            .Where(c => c.Mercado == TipoMercado.Ganador && Resultados1X2.Contains(c.Resultado) && c.Valor > 1m)
            .GroupBy(c => c.Resultado)
            .Select(g => g.MaxBy(c => c.Valor)!)
            .OrderBy(c => c.Resultado)
            .ToList();
}
