namespace OddsArbitrage.Api.Domain;

public sealed record Cuota
{
    public required CasaDeApuestas Casa { get; init; }

    public TipoMercado Mercado { get; init; } = TipoMercado.Ganador;

    // Punto del mercado (ej. 2.5 goles en totales o -1.5 en handicap); null en el mercado ganador
    public decimal? Linea { get; init; }

    public required ResultadoCuota Resultado { get; init; }

    // Cuota en formato decimal (ej. 2.10)
    public required decimal Valor { get; init; }

    public required DateTimeOffset UltimaActualizacion { get; init; }
}
