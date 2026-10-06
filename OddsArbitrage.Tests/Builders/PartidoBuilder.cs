using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Tests.Builders;

// Builder sencillo para construir un Partido de prueba con cuotas de varias casas,
// sin depender de The Odds API.
public sealed class PartidoBuilder
{
    private string _id = "partido-prueba-1";
    private string _liga = "Liga de Prueba";
    private string _equipoLocal = "Equipo Local";
    private string _equipoVisitante = "Equipo Visitante";
    private DateTimeOffset _fechaInicio = new(2026, 1, 1, 20, 0, 0, TimeSpan.Zero);
    private readonly List<Cuota> _cuotas = [];

    public PartidoBuilder ConId(string id)
    {
        _id = id;
        return this;
    }

    public PartidoBuilder ConCuota(string casaClave, ResultadoPartido resultado, decimal valor)
    {
        _cuotas.Add(new Cuota
        {
            Casa = new CasaDeApuestas { Clave = casaClave, Nombre = casaClave },
            Resultado = resultado,
            Valor = valor,
            UltimaActualizacion = _fechaInicio
        });
        return this;
    }

    public Partido Build() => new()
    {
        Id = _id,
        Liga = _liga,
        EquipoLocal = _equipoLocal,
        EquipoVisitante = _equipoVisitante,
        FechaInicio = _fechaInicio,
        Cuotas = _cuotas
    };
}
