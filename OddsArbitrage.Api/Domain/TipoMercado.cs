namespace OddsArbitrage.Api.Domain;

// Mercados soportados; cada uno equivale a una clave de The Odds API
public enum TipoMercado
{
    // Ganador del partido (h2h), el 1X2 cuando hay empate
    Ganador,

    // Total de goles o puntos por encima o por debajo de una linea (totals)
    Totales,

    // Handicap sobre el resultado de un equipo (spreads)
    Handicap
}
