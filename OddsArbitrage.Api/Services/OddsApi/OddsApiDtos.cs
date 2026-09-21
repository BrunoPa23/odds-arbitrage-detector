namespace OddsArbitrage.Api.Services.OddsApi;

// Reflejan la respuesta de GET /v4/sports/{sport}/odds de The Odds API

internal sealed record EventDto(
    string Id,
    string SportKey,
    string SportTitle,
    DateTimeOffset CommenceTime,
    string HomeTeam,
    string AwayTeam,
    IReadOnlyList<BookmakerDto> Bookmakers);

internal sealed record BookmakerDto(
    string Key,
    string Title,
    DateTimeOffset LastUpdate,
    IReadOnlyList<MarketDto> Markets);

internal sealed record MarketDto(
    string Key,
    DateTimeOffset? LastUpdate,
    IReadOnlyList<OutcomeDto> Outcomes);

internal sealed record OutcomeDto(
    string Name,
    decimal Price);
