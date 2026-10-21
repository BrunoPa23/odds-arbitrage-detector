namespace OddsArbitrage.Api.Domain;

// Cuota claramente por encima del consenso del mercado (value bet). No es ganancia garantizada:
// es una ventaja esperada si la probabilidad justa del consenso es correcta
public sealed record CuotaDesfasada
{
    public required string PartidoId { get; init; }
    public required string Liga { get; init; }
    public required string EquipoLocal { get; init; }
    public required string EquipoVisitante { get; init; }
    public required DateTimeOffset FechaInicio { get; init; }

    public required TipoMercado Mercado { get; init; }

    // Linea de la propia cuota tal como se apuesta (en handicap el visitante tiene la del local con signo opuesto)
    public decimal? Linea { get; init; }

    public required ResultadoCuota Resultado { get; init; }
    public required CasaDeApuestas Casa { get; init; }

    // Cuota decimal que ofrece la casa
    public required decimal Valor { get; init; }

    public required DateTimeOffset UltimaActualizacion { get; init; }

    // Probabilidad del resultado segun el consenso sin margen
    public required decimal ProbabilidadJusta { get; init; }

    // Cuota que corresponde a la probabilidad justa: 1 / probabilidad
    public required decimal CuotaJusta { get; init; }

    // Ganancia esperada por unidad apostada en porcentaje: (cuota x probabilidad justa - 1) x 100
    public required decimal VentajaPorcentaje { get; init; }

    // Casas que forman el consenso
    public required int CasasConsenso { get; init; }
}
