using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;

namespace OddsArbitrage.Api.Services.ValueBets;

// Probabilidad justa (sin margen de la casa) de cada resultado, a partir del consenso de varias casas
public static class ProbabilidadJusta
{
    // Con menos casas el promedio depende demasiado de una sola y deja de ser un consenso confiable
    public const int MinimoCasasPorDefecto = 3;

    // Metodo proporcional: p_i = (1 / cuota_i) / suma(1 / cuota_j). Reparte el margen de la casa
    // en proporcion a cada probabilidad implicita, asi las probabilidades de la casa suman 1.
    // Espera una cuota por resultado que cubra el mercado completo de una sola casa
    public static IReadOnlyDictionary<ResultadoCuota, decimal> QuitarMargen(IReadOnlyCollection<Cuota> cuotasDeUnaCasa)
    {
        if (cuotasDeUnaCasa.Count == 0)
        {
            throw new ArgumentException("Se necesita al menos una cuota", nameof(cuotasDeUnaCasa));
        }

        var suma = ProbabilidadImplicita.CalcularSuma(cuotasDeUnaCasa);

        return cuotasDeUnaCasa.ToDictionary(
            c => c.Resultado,
            c => ProbabilidadImplicita.Calcular(c.Valor) / suma);
    }

    // Consenso de todos los mercados y lineas del partido que tengan suficientes casas
    public static IReadOnlyList<ConsensoMercado> CalcularConsensos(Partido partido, int minimoCasas = MinimoCasasPorDefecto)
        => LineasDeMercado.Agrupar(partido.Cuotas)
            .Select(grupo => CalcularConsenso(grupo.Key.Mercado, grupo.Key.Linea, grupo, minimoCasas))
            .OfType<ConsensoMercado>()
            .OrderBy(c => c.Mercado)
            .ThenBy(c => c.Linea)
            .ToList();

    // Consenso de un mercado y linea concretos (en handicap, la linea del local), o null si no hay suficientes casas
    public static ConsensoMercado? CalcularConsenso(
        Partido partido, TipoMercado mercado, decimal? linea, int minimoCasas = MinimoCasasPorDefecto)
    {
        var cuotas = LineasDeMercado.Agrupar(partido.Cuotas)
            .FirstOrDefault(g => g.Key.Mercado == mercado && g.Key.Linea == linea);

        return cuotas is null ? null : CalcularConsenso(mercado, linea, cuotas, minimoCasas);
    }

    // Quita el margen de cada casa por separado y promedia entre casas; como cada casa suma 1, el promedio tambien
    internal static ConsensoMercado? CalcularConsenso(
        TipoMercado mercado, decimal? linea, IEnumerable<Cuota> cuotasDelGrupo, int minimoCasas)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minimoCasas, 1);

        var resultados = ResultadosPorMercado.De(mercado);

        var probabilidadesPorCasa = cuotasDelGrupo
            .GroupBy(c => c.Casa.Clave)
            // Si una casa repite un resultado vale la cotizacion mas reciente
            .Select(casa => casa
                .GroupBy(c => c.Resultado)
                .Select(r => r.MaxBy(c => c.UltimaActualizacion)!)
                .ToList())
            // Sin todos los resultados no se puede saber cual es el margen de la casa
            .Where(cuotasCasa => cuotasCasa.Count == resultados.Count)
            .Select(QuitarMargen)
            .ToList();

        if (probabilidadesPorCasa.Count < minimoCasas)
        {
            return null;
        }

        return new ConsensoMercado
        {
            Mercado = mercado,
            Linea = linea,
            Casas = probabilidadesPorCasa.Count,
            ProbabilidadesJustas = resultados.ToDictionary(
                r => r,
                r => probabilidadesPorCasa.Average(p => p[r]))
        };
    }
}
