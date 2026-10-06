using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

public sealed class DetectorArbitraje : IDetectorArbitraje
{
    public const decimal StakePorDefecto = 100m;

    private static readonly int CantidadResultados1X2 = Enum.GetValues<ResultadoPartido>().Length;

    public OportunidadArbitraje? Evaluar(Partido partido, decimal stakeTotal = StakePorDefecto)
    {
        var mejoresCuotas = SeleccionarMejoresCuotas(partido.Cuotas);

        // Si falta algun resultado no se puede cubrir el mercado completo, por lo tanto no hay arbitraje
        if (mejoresCuotas.Count != CantidadResultados1X2)
        {
            return null;
        }

        var sumaProbabilidades = ProbabilidadImplicita.CalcularSuma(mejoresCuotas);
        if (sumaProbabilidades >= 1m)
        {
            return null;
        }

        return new OportunidadArbitraje
        {
            PartidoId = partido.Id,
            Liga = partido.Liga,
            EquipoLocal = partido.EquipoLocal,
            EquipoVisitante = partido.EquipoVisitante,
            FechaInicio = partido.FechaInicio,
            MejoresCuotas = mejoresCuotas,
            SumaProbabilidadesImplicitas = Math.Round(sumaProbabilidades, 6),
            MargenPorcentaje = Math.Round((1m / sumaProbabilidades - 1m) * 100m, 2),
            Reparto = RepartidorStake.Calcular(mejoresCuotas, stakeTotal)
        };
    }

    public IReadOnlyList<OportunidadArbitraje> Detectar(IEnumerable<Partido> partidos, decimal stakeTotal = StakePorDefecto)
        => partidos
            .Select(p => Evaluar(p, stakeTotal))
            .OfType<OportunidadArbitraje>()
            .OrderByDescending(o => o.MargenPorcentaje)
            .ToList();

    // Para cada resultado (Local, Empate, Visitante) toma la cuota mas alta entre todas las casas
    private static List<Cuota> SeleccionarMejoresCuotas(IEnumerable<Cuota> cuotas)
        => cuotas
            .Where(c => c.Valor > 1m)
            .GroupBy(c => c.Resultado)
            .Select(g => g.MaxBy(c => c.Valor)!)
            .OrderBy(c => c.Resultado)
            .ToList();
}
