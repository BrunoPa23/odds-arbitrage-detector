namespace OddsArbitrage.Api.Domain;

public sealed record ApuestaRecomendada
{
    public required Cuota Cuota { get; init; }

    // Monto a apostar en esta cuota
    public required decimal Stake { get; init; }

    // Monto que se recibe si este resultado ocurre (Stake x Valor de la cuota)
    public required decimal Retorno { get; init; }
}

public sealed record RepartoStake
{
    public required decimal StakeTotal { get; init; }
    public required IReadOnlyList<ApuestaRecomendada> Apuestas { get; init; }

    // Retorno minimo entre todas las apuestas; es lo que se cobra sin importar el resultado
    public required decimal RetornoGarantizado { get; init; }
    public required decimal GananciaGarantizada { get; init; }
}
