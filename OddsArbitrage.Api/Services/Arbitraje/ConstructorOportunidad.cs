using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

// Logica comun a todos los mercados: dadas las mejores cuotas que cubren el mercado completo,
// decide si hay arbitraje y arma la oportunidad con su margen y reparto de stake
internal static class ConstructorOportunidad
{
    public static OportunidadArbitraje? Crear(
        Partido partido,
        TipoMercado mercado,
        decimal? linea,
        IReadOnlyList<Cuota> mejoresCuotas,
        decimal stakeTotal)
    {
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
            Mercado = mercado,
            Linea = linea,
            MejoresCuotas = mejoresCuotas,
            SumaProbabilidadesImplicitas = Math.Round(sumaProbabilidades, 6),
            MargenPorcentaje = Math.Round((1m / sumaProbabilidades - 1m) * 100m, 2),
            Reparto = RepartidorStake.Calcular(mejoresCuotas, stakeTotal)
        };
    }
}
