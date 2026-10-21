using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;

namespace OddsArbitrage.Api.Services.ValueBets;

// Una cuota esta desfasada cuando paga mas de lo que vale segun el consenso:
// ventaja = cuota x probabilidad justa - 1. Sirve para ganador, totales y handicap
public sealed class DetectorCuotasDesfasadas : IDetectorCuotasDesfasadas
{
    public IReadOnlyList<CuotaDesfasada> Detectar(
        IEnumerable<Partido> partidos,
        TipoMercado? mercado = null,
        decimal ventajaMinima = IDetectorCuotasDesfasadas.VentajaMinimaPorDefecto,
        int minimoCasas = ProbabilidadJusta.MinimoCasasPorDefecto)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ventajaMinima);
        ArgumentOutOfRangeException.ThrowIfLessThan(minimoCasas, 1);

        return partidos
            .SelectMany(p => Evaluar(p, mercado, ventajaMinima, minimoCasas))
            .OrderByDescending(c => c.VentajaPorcentaje)
            .ThenBy(c => c.FechaInicio)
            .ThenBy(c => c.PartidoId)
            .ThenBy(c => c.Casa.Clave)
            .ToList();
    }

    private static IEnumerable<CuotaDesfasada> Evaluar(
        Partido partido, TipoMercado? mercado, decimal ventajaMinima, int minimoCasas)
    {
        var grupos = LineasDeMercado.Agrupar(partido.Cuotas)
            .Where(g => mercado is null || g.Key.Mercado == mercado);

        foreach (var grupo in grupos)
        {
            var consenso = ProbabilidadJusta.CalcularConsenso(grupo.Key.Mercado, grupo.Key.Linea, grupo, minimoCasas);
            if (consenso is null)
            {
                continue;
            }

            foreach (var cuota in grupo)
            {
                var probabilidad = consenso.ProbabilidadesJustas[cuota.Resultado];
                var ventaja = Math.Round((cuota.Valor * probabilidad - 1m) * 100m, 2);

                // Una ventaja nula o negativa nunca es desfasada, aunque el umbral sea 0
                if (ventaja > 0m && ventaja >= ventajaMinima)
                {
                    yield return Crear(partido, cuota, consenso, probabilidad, ventaja);
                }
            }
        }
    }

    private static CuotaDesfasada Crear(
        Partido partido, Cuota cuota, ConsensoMercado consenso, decimal probabilidad, decimal ventaja) => new()
    {
        PartidoId = partido.Id,
        Liga = partido.Liga,
        EquipoLocal = partido.EquipoLocal,
        EquipoVisitante = partido.EquipoVisitante,
        FechaInicio = partido.FechaInicio,
        Mercado = cuota.Mercado,
        Linea = cuota.Linea,
        Resultado = cuota.Resultado,
        Casa = cuota.Casa,
        Valor = cuota.Valor,
        UltimaActualizacion = cuota.UltimaActualizacion,
        ProbabilidadJusta = Math.Round(probabilidad, 6),
        CuotaJusta = Math.Round(1m / probabilidad, 3),
        VentajaPorcentaje = ventaja,
        CasasConsenso = consenso.Casas
    };
}
