using System.ComponentModel.DataAnnotations;

namespace OddsArbitrage.Api.Services.OddsApi;

public sealed class OddsApiOptions
{
    public const string Seccion = "TheOddsApi";

    [Required, Url]
    public string BaseUrl { get; set; } = "https://api.the-odds-api.com/";

    // Se configura con user-secrets, nunca en appsettings.json
    [Required]
    public string ApiKey { get; set; } = string.Empty;

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
}
