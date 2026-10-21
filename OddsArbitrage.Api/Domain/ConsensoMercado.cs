namespace OddsArbitrage.Api.Domain;

// Probabilidad justa de cada resultado de un mercado segun el consenso de varias casas, ya sin su margen
public sealed record ConsensoMercado
{
    public required TipoMercado Mercado { get; init; }

    // Linea del grupo: el total en totales y la linea del local en handicap; null en el mercado ganador
    public decimal? Linea { get; init; }

    // Casas que cotizan todos los resultados del mercado y entran en el promedio
    public required int Casas { get; init; }

    // Probabilidades justas por resultado; suman 1
    public required IReadOnlyDictionary<ResultadoCuota, decimal> ProbabilidadesJustas { get; init; }
}
