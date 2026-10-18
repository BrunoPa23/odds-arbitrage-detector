namespace OddsArbitrage.Api.Services.OddsApi;

// Claves de mercado que acepta el parametro markets de The Odds API
public static class MercadosOddsApi
{
    public const string H2H = "h2h";
    public const string Totals = "totals";
    public const string Spreads = "spreads";

    public static readonly IReadOnlyList<string> Soportados = [H2H, Totals, Spreads];
}
