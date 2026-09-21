namespace OddsArbitrage.Api.Domain;

public sealed record Cuota
{
    public required CasaDeApuestas Casa { get; init; }
    public required ResultadoPartido Resultado { get; init; }

    // Cuota en formato decimal (ej. 2.10)
    public required decimal Valor { get; init; }

    public required DateTimeOffset UltimaActualizacion { get; init; }
}
