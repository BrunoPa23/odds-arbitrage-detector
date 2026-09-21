using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.OddsApi;

internal static class OddsApiMapper
{
    public const string MercadoH2H = "h2h";
    private const string NombreEmpate = "Draw";

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
            where mercado.Key == MercadoH2H
            from outcome in mercado.Outcomes
            let resultado = AResultado(outcome.Name, evento)
            where resultado is not null
            select new Cuota
            {
                Casa = casa,
                Resultado = resultado.Value,
                Valor = outcome.Price,
                UltimaActualizacion = mercado.LastUpdate ?? bookmaker.LastUpdate
            };
    }

    private static ResultadoPartido? AResultado(string nombre, EventDto evento) => nombre switch
    {
        _ when nombre == evento.HomeTeam => ResultadoPartido.Local,
        _ when nombre == evento.AwayTeam => ResultadoPartido.Visitante,
        NombreEmpate => ResultadoPartido.Empate,
        _ => null
    };
}
