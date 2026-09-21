namespace OddsArbitrage.Api.Domain;

public sealed record Partido
{
    public required string Id { get; init; }
    public required string Liga { get; init; }
    public required string EquipoLocal { get; init; }
    public required string EquipoVisitante { get; init; }
    public required DateTimeOffset FechaInicio { get; init; }
    public IReadOnlyList<Cuota> Cuotas { get; init; } = [];
}
