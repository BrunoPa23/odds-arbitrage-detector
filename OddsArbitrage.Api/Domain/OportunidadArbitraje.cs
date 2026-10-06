namespace OddsArbitrage.Api.Domain;

public sealed record OportunidadArbitraje
{
    public required string PartidoId { get; init; }
    public required string Liga { get; init; }
    public required string EquipoLocal { get; init; }
    public required string EquipoVisitante { get; init; }
    public required DateTimeOffset FechaInicio { get; init; }

    // Mejor cuota disponible para cada resultado del mercado 1X2 (una por resultado)
    public required IReadOnlyList<Cuota> MejoresCuotas { get; init; }

    // Suma de probabilidades implicitas de las mejores cuotas; hay arbitraje cuando es menor a 1
    public required decimal SumaProbabilidadesImplicitas { get; init; }

    // Ganancia garantizada expresada como porcentaje del stake total (ej. 2.5 equivale a 2.5 %)
    public required decimal MargenPorcentaje { get; init; }

    // Como repartir el stake entre las mejores cuotas para asegurar la ganancia
    public required RepartoStake Reparto { get; init; }
}
