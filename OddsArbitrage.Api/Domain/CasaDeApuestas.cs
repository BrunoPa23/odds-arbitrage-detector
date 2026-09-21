namespace OddsArbitrage.Api.Domain;

public sealed record CasaDeApuestas
{
    public required string Clave { get; init; }
    public required string Nombre { get; init; }
}
