using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

// Detecta surebets en mercados de dos resultados: totales (mas o menos) y handicap (local o visitante).
// Cada linea es un mercado independiente, por lo que un mismo partido puede tener varias surebets
public sealed class DetectorDosResultados : IDetectorSurebets
{
    private static readonly TipoMercado[] MercadosSoportados = [TipoMercado.Totales, TipoMercado.Handicap];

    public IReadOnlyCollection<TipoMercado> Mercados => MercadosSoportados;

    public IReadOnlyList<OportunidadArbitraje> Evaluar(Partido partido, decimal stakeTotal = IDetectorSurebets.StakePorDefecto)
        => partido.Cuotas
            .Where(EsCuotaEvaluable)
            .GroupBy(c => (c.Mercado, Linea: LineaDelGrupo(c)))
            .Select(grupo => EvaluarLinea(partido, grupo.Key.Mercado, grupo.Key.Linea, grupo, stakeTotal))
            .OfType<OportunidadArbitraje>()
            .OrderBy(o => o.Mercado)
            .ThenBy(o => o.Linea)
            .ToList();

    public IReadOnlyList<OportunidadArbitraje> Detectar(IEnumerable<Partido> partidos, decimal stakeTotal = IDetectorSurebets.StakePorDefecto)
        => partidos
            .SelectMany(p => Evaluar(p, stakeTotal))
            .OrderByDescending(o => o.MargenPorcentaje)
            .ToList();

    // En totales ambos lados comparten la linea (mas de 2.5 contra menos de 2.5). En handicap el visitante
    // tiene la linea opuesta (local -1.5 se cubre con visitante +1.5), por eso el grupo usa la linea del local
    internal static decimal LineaDelGrupo(Cuota cuota)
    {
        var linea = cuota.Mercado == TipoMercado.Handicap && cuota.Resultado == ResultadoCuota.Visitante
            ? -cuota.Linea!.Value
            : cuota.Linea!.Value;

        // Evita el cero negativo para que la linea 0 se agrupe y se serialice siempre igual
        return linea == 0m ? 0m : linea;
    }

    private static bool EsCuotaEvaluable(Cuota cuota)
        => MercadosSoportados.Contains(cuota.Mercado)
           && cuota.Linea is not null
           && cuota.Valor > 1m
           && ResultadosPorMercado.EsValido(cuota.Mercado, cuota.Resultado);

    // Toma la mejor cuota de cada lado entre todas las casas; si falta un lado no se cubre el mercado
    private static OportunidadArbitraje? EvaluarLinea(
        Partido partido, TipoMercado mercado, decimal linea, IEnumerable<Cuota> cuotas, decimal stakeTotal)
    {
        var mejoresCuotas = cuotas
            .GroupBy(c => c.Resultado)
            .Select(g => g.MaxBy(c => c.Valor)!)
            .OrderBy(c => c.Resultado)
            .ToList();

        if (mejoresCuotas.Count != ResultadosPorMercado.De(mercado).Count)
        {
            return null;
        }

        return ConstructorOportunidad.Crear(partido, mercado, linea, mejoresCuotas, stakeTotal);
    }
}
