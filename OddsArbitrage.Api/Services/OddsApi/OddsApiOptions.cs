using System.ComponentModel.DataAnnotations;

namespace OddsArbitrage.Api.Services.OddsApi;

public sealed class OddsApiOptions : IValidatableObject
{
    public const string Seccion = "TheOddsApi";

    [Required, Url]
    public string BaseUrl { get; set; } = "https://api.the-odds-api.com/";

    // Se configura con user-secrets, nunca en appsettings.json. Es obligatoria salvo en modo demo
    public string ApiKey { get; set; } = string.Empty;

    // Con true la API sirve partidos de ejemplo (Datos/cuotas-demo.json) y nunca llama a The Odds API
    public bool ModoDemo { get; set; }

    // Cada region extra multiplica el costo de cada llamada; mantener una sola
    [Required]
    public string Region { get; set; } = "eu";

    [Required]
    public string DeportePorDefecto { get; set; } = "soccer_epl";

    // Umbral de requests restantes a partir del cual se loguea un warning
    [Range(0, 500)]
    public int UmbralAlertaCuota { get; set; } = 50;

    // Tiempo que se reutiliza una respuesta antes de volver a consultar la API
    [Range(1, 1440)]
    public int MinutosCache { get; set; } = 30;

    // Mercados a consultar (h2h, totals, spreads). Cada mercado extra multiplica el costo de cada llamada,
    // por eso si la lista queda vacia solo se pide h2h. Arranca vacia porque el binder agrega a la lista existente
    public List<string> Mercados { get; set; } = [];

    // Mercados normalizados y sin repetir, listos para el parametro markets
    public IReadOnlyList<string> MercadosAConsultar()
    {
        var mercados = Mercados
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(m => m.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        return mercados.Count > 0 ? mercados : [MercadosOddsApi.H2H];
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ModoDemo && string.IsNullOrWhiteSpace(ApiKey))
        {
            yield return new ValidationResult(
                "Falta TheOddsApi:ApiKey. Configurala con user-secrets o activa TheOddsApi:ModoDemo para usar datos de ejemplo",
                [nameof(ApiKey)]);
        }

        var noSoportados = MercadosAConsultar().Except(MercadosOddsApi.Soportados).ToList();
        if (noSoportados.Count > 0)
        {
            yield return new ValidationResult(
                $"Mercados no soportados: {string.Join(", ", noSoportados)}. Validos: {string.Join(", ", MercadosOddsApi.Soportados)}",
                [nameof(Mercados)]);
        }
    }
}
