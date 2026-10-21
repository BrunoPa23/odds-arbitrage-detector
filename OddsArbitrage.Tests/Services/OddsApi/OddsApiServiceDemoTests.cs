using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Domain;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Api.Services.OddsApi;
using OddsArbitrage.Api.Services.ValueBets;

namespace OddsArbitrage.Tests.Services.OddsApi;

public class OddsApiServiceDemoTests
{
    // Dia en que se capturaron los datos del fixture (fecha_captura)
    private static readonly DateTimeOffset DiaCaptura = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ObtenerPartidos_SinDeporte_DevuelveLosDelDeportePorDefecto()
    {
        // Arrange
        var servicio = CrearServicio(DiaCaptura);

        // Act
        var partidos = await servicio.ObtenerPartidosAsync();

        // Assert
        Assert.Equal(4, partidos.Count);
        Assert.All(partidos, p => Assert.Equal("EPL", p.Liga));
    }

    [Theory]
    [InlineData("soccer_spain_la_liga", "La Liga - Spain")]
    [InlineData("soccer_italy_serie_a", "Serie A - Italy")]
    [InlineData("soccer_germany_bundesliga", "Bundesliga - Germany")]
    [InlineData("basketball_nba", "NBA")]
    public async Task ObtenerPartidos_ConDeporte_FiltraPorLiga(string deporte, string liga)
    {
        // Arrange
        var servicio = CrearServicio(DiaCaptura);

        // Act
        var partidos = await servicio.ObtenerPartidosAsync(deporte);

        // Assert
        Assert.NotEmpty(partidos);
        Assert.All(partidos, p => Assert.Equal(liga, p.Liga));
    }

    [Fact]
    public async Task ObtenerPartidos_ConDeporteSinDatos_DevuelveListaVacia()
    {
        // Arrange
        var servicio = CrearServicio(DiaCaptura);

        // Act
        var partidos = await servicio.ObtenerPartidosAsync("tennis_atp_paris");

        // Assert
        Assert.Empty(partidos);
    }

    [Fact]
    public async Task ObtenerPartidos_IncluyeLosTresMercadosYAlMenosCuatroCasas()
    {
        // Arrange
        var servicio = CrearServicio(DiaCaptura);

        // Act
        var partidos = await servicio.ObtenerPartidosAsync("soccer_epl");

        // Assert
        Assert.All(partidos, p =>
        {
            Assert.Equal(
                [TipoMercado.Ganador, TipoMercado.Totales, TipoMercado.Handicap],
                p.Cuotas.Select(c => c.Mercado).Distinct().Order());
            Assert.True(p.Cuotas.Select(c => c.Casa.Clave).Distinct().Count() >= 4);
        });
    }

    [Fact]
    public async Task ObtenerPartidos_DiasDespuesDeLaCaptura_DesplazaLasFechas()
    {
        // Arrange: el mismo partido consultado el dia de la captura y diez dias despues
        var original = (await CrearServicio(DiaCaptura).ObtenerPartidosAsync()).First();
        var servicio = CrearServicio(DiaCaptura.AddDays(10));

        // Act
        var desplazado = (await servicio.ObtenerPartidosAsync()).First(p => p.Id == original.Id);

        // Assert
        Assert.Equal(original.FechaInicio.AddDays(10), desplazado.FechaInicio);
        Assert.Equal(
            original.Cuotas[0].UltimaActualizacion.AddDays(10),
            desplazado.Cuotas[0].UltimaActualizacion);
    }

    [Fact]
    public async Task Fixture_TieneSurebetsEnCadaMercado()
    {
        // Arrange
        var buscador = new BuscadorSurebets([new DetectorArbitraje(), new DetectorDosResultados()]);
        var partidos = await TodosLosPartidosAsync();

        // Act
        var surebets = buscador.Buscar(partidos);

        // Assert
        Assert.Contains(surebets, s => s.Mercado == TipoMercado.Ganador);
        Assert.Contains(surebets, s => s.Mercado == TipoMercado.Totales);
        Assert.Contains(surebets, s => s.Mercado == TipoMercado.Handicap);
        Assert.All(surebets, s => Assert.InRange(s.MargenPorcentaje, 0.01m, 5m));
    }

    [Fact]
    public async Task Fixture_TieneVariasCuotasDesfasadas()
    {
        // Arrange
        var detector = new DetectorCuotasDesfasadas();
        var partidos = await TodosLosPartidosAsync();

        // Act
        var desfasadas = detector.Detectar(partidos);

        // Assert
        Assert.True(desfasadas.Count >= 5);
        Assert.True(desfasadas.Select(d => d.PartidoId).Distinct().Count() >= 5);
    }

    private static async Task<List<Partido>> TodosLosPartidosAsync()
    {
        var servicio = CrearServicio(DiaCaptura);
        string[] deportes = ["soccer_epl", "soccer_spain_la_liga", "soccer_italy_serie_a", "soccer_germany_bundesliga", "basketball_nba"];

        var partidos = new List<Partido>();
        foreach (var deporte in deportes)
        {
            partidos.AddRange(await servicio.ObtenerPartidosAsync(deporte));
        }

        return partidos;
    }

    private static OddsApiServiceDemo CrearServicio(DateTimeOffset ahora)
        => new(
            new RelojFijo(ahora),
            Options.Create(new OddsApiOptions { ModoDemo = true }),
            NullLogger<OddsApiServiceDemo>.Instance);

    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;
    }
}
