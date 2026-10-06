using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

public static class RepartidorStake
{
    // Reparte el stake total de forma proporcional a la probabilidad implicita de cada cuota.
    // Asi el retorno es el mismo sin importar que resultado ocurra: stake_i = total x (1 / cuota_i) / suma
    public static RepartoStake Calcular(IReadOnlyList<Cuota> mejoresCuotas, decimal stakeTotal)
    {
        if (stakeTotal <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(stakeTotal), stakeTotal, "El stake total debe ser mayor a 0");
        }

        var sumaProbabilidades = ProbabilidadImplicita.CalcularSuma(mejoresCuotas);

        var apuestas = mejoresCuotas
            .Select(cuota =>
            {
                var stake = Math.Round(stakeTotal * ProbabilidadImplicita.Calcular(cuota.Valor) / sumaProbabilidades, 2);
                return new ApuestaRecomendada
                {
                    Cuota = cuota,
                    Stake = stake,
                    Retorno = Math.Round(stake * cuota.Valor, 2)
                };
            })
            .ToList();

        // Se usa la suma de los stakes redondeados para que la ganancia reportada sea exacta
        var stakeRepartido = apuestas.Sum(a => a.Stake);
        var retornoGarantizado = apuestas.Min(a => a.Retorno);

        return new RepartoStake
        {
            StakeTotal = stakeRepartido,
            Apuestas = apuestas,
            RetornoGarantizado = retornoGarantizado,
            GananciaGarantizada = Math.Round(retornoGarantizado - stakeRepartido, 2)
        };
    }
}
