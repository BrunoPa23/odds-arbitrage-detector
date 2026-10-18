using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.OddsApi;

internal static class OddsApiMapper
{
    private const string NombreEmpate = "Draw";
    private const string NombreMas = "Over";
    private const string NombreMenos = "Under";

    public static Partido APartido(EventDto evento) => new()
    {
        Id = evento.Id,
        Liga = evento.SportTitle,
        EquipoLocal = evento.HomeTeam,
        EquipoVisitante = evento.AwayTeam,
        FechaInicio = evento.CommenceTime,
        Cuotas = evento.Bookmakers
            .SelectMany(bookmaker => ACuotas(bookmaker, evento))
            .ToList()
    };

    private static IEnumerable<Cuota> ACuotas(BookmakerDto bookmaker, EventDto evento)
    {
        var casa = new CasaDeApuestas { Clave = bookmaker.Key, Nombre = bookmaker.Title };

        return
            from mercado in bookmaker.Markets
            let tipo = ATipoMercado(mercado.Key)
            where tipo is not null
            from outcome in mercado.Outcomes
            let resultado = AResultado(tipo.Value, outcome.Name, evento)
            where resultado is not null && TieneLinea(tipo.Value, outcome.Point)
            select new Cuota
            {
                Casa = casa,
                Mercado = tipo.Value,
                Linea = tipo.Value == TipoMercado.Ganador ? null : outcome.Point,
                Resultado = resultado.Value,
                Valor = outcome.Price,
                UltimaActualizacion = mercado.LastUpdate ?? bookmaker.LastUpdate
            };
    }

    private static TipoMercado? ATipoMercado(string clave) => clave switch
    {
        MercadosOddsApi.H2H => TipoMercado.Ganador,
        MercadosOddsApi.Totals => TipoMercado.Totales,
        MercadosOddsApi.Spreads => TipoMercado.Handicap,
        _ => null
    };

    // En totales el outcome es Over o Under; en h2h y spreads es el nombre de un equipo (o Draw en h2h)
    private static ResultadoCuota? AResultado(TipoMercado mercado, string nombre, EventDto evento) => mercado switch
    {
        TipoMercado.Totales when nombre == NombreMas => ResultadoCuota.Mas,
        TipoMercado.Totales when nombre == NombreMenos => ResultadoCuota.Menos,
        TipoMercado.Totales => null,
        _ when nombre == evento.HomeTeam => ResultadoCuota.Local,
        _ when nombre == evento.AwayTeam => ResultadoCuota.Visitante,
        TipoMercado.Ganador when nombre == NombreEmpate => ResultadoCuota.Empate,
        _ => null
    };

    // Totales y handicap necesitan el punto para saber a que linea pertenece la cuota
    private static bool TieneLinea(TipoMercado mercado, decimal? punto)
        => mercado == TipoMercado.Ganador || punto is not null;
}
