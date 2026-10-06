using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.Arbitraje;

public static class ProbabilidadImplicita
{
    // Probabilidad implicita de una cuota decimal: 1 / cuota (ej. cuota 2.00 implica 50 %)
    public static decimal Calcular(decimal cuotaDecimal)
    {
        if (cuotaDecimal <= 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cuotaDecimal), cuotaDecimal, "La cuota decimal debe ser mayor a 1");
        }

        return 1m / cuotaDecimal;
    }

    // Suma de probabilidades implicitas de un conjunto de cuotas.
    // Si las cuotas cubren todos los resultados del mercado y la suma es menor a 1, existe arbitraje.
    public static decimal CalcularSuma(IEnumerable<Cuota> cuotas)
        => cuotas.Sum(c => Calcular(c.Valor));
}
