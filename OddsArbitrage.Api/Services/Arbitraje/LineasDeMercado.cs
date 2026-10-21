using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

// Agrupa las cuotas de un partido en mercados independientes (mercado y linea).
// Lo comparten los detectores de surebets y el consenso sin margen para que emparejen las lineas igual
internal static class LineasDeMercado
{
    public static IEnumerable<IGrouping<(TipoMercado Mercado, decimal? Linea), Cuota>> Agrupar(IEnumerable<Cuota> cuotas)
        => cuotas
            .Where(EsCuotaValida)
            .GroupBy(c => (c.Mercado, LineaDelGrupo(c)));

    // En ganador no hay linea. En totales ambos lados comparten la linea (mas de 2.5 contra menos de 2.5).
    // En handicap el visitante tiene la linea opuesta (local -1.5 se cubre con visitante +1.5),
    // por eso el grupo usa la linea del local
    internal static decimal? LineaDelGrupo(Cuota cuota)
    {
        if (cuota.Mercado == TipoMercado.Ganador)
        {
            return null;
        }

        var linea = cuota.Mercado == TipoMercado.Handicap && cuota.Resultado == ResultadoCuota.Visitante
            ? -cuota.Linea!.Value
            : cuota.Linea!.Value;

        // Evita el cero negativo para que la linea 0 se agrupe y se serialice siempre igual
        return linea == 0m ? 0m : linea;
    }

    // Solo cuentan cuotas reales (mayores a 1), con un resultado valido para su mercado
    // y con linea en los mercados que la necesitan
    private static bool EsCuotaValida(Cuota cuota)
        => cuota.Valor > 1m
           && ResultadosPorMercado.EsValido(cuota.Mercado, cuota.Resultado)
           && (cuota.Mercado == TipoMercado.Ganador || cuota.Linea is not null);
}
