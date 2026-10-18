namespace OddsArbitrage.Api.Domain;

// Resultados que cubren por completo cada mercado; si se apuesta a todos, siempre gana uno
public static class ResultadosPorMercado
{
    private static readonly IReadOnlyList<ResultadoCuota> Ganador =
        [ResultadoCuota.Local, ResultadoCuota.Empate, ResultadoCuota.Visitante];

    private static readonly IReadOnlyList<ResultadoCuota> Totales =
        [ResultadoCuota.Mas, ResultadoCuota.Menos];

    private static readonly IReadOnlyList<ResultadoCuota> Handicap =
        [ResultadoCuota.Local, ResultadoCuota.Visitante];

    public static IReadOnlyList<ResultadoCuota> De(TipoMercado mercado) => mercado switch
    {
        TipoMercado.Ganador => Ganador,
        TipoMercado.Totales => Totales,
        TipoMercado.Handicap => Handicap,
        _ => throw new ArgumentOutOfRangeException(nameof(mercado), mercado, "Mercado no soportado")
    };

    public static bool EsValido(TipoMercado mercado, ResultadoCuota resultado) => De(mercado).Contains(resultado);
}
