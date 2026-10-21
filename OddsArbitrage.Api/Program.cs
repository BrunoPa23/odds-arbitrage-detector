using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Filters;
using OddsArbitrage.Api.Services.Arbitraje;
using OddsArbitrage.Api.Services.OddsApi;
using OddsArbitrage.Api.Services.ValueBets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

const string PoliticaCorsWeb = "PoliticaCorsWeb";
var origenesPermitidos = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCorsWeb, policy =>
    {
        policy.WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddOptions<OddsApiOptions>()
    .Bind(builder.Configuration.GetSection(OddsApiOptions.Seccion))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<ValueBetsOptions>()
    .Bind(builder.Configuration.GetSection(ValueBetsOptions.Seccion))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Sin politica de reintentos a proposito: cada reintento gasta cuota de la API
builder.Services.AddHttpClient<OddsApiService>((sp, client) =>
{
    var opciones = sp.GetRequiredService<IOptions<OddsApiOptions>>().Value;
    client.BaseAddress = new Uri(opciones.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OddsApiServiceConCache>();
builder.Services.AddSingleton<OddsApiServiceDemo>();
// Con TheOddsApi:ModoDemo=true se sirven datos de ejemplo y nunca se llama a The Odds API
builder.Services.AddScoped<IOddsApiService>(sp =>
    sp.GetRequiredService<IOptions<OddsApiOptions>>().Value.ModoDemo
        ? sp.GetRequiredService<OddsApiServiceDemo>()
        : sp.GetRequiredService<OddsApiServiceConCache>());
builder.Services.AddSingleton<IDetectorSurebets, DetectorArbitraje>();
builder.Services.AddSingleton<IDetectorSurebets, DetectorDosResultados>();
builder.Services.AddSingleton<IBuscadorSurebets, BuscadorSurebets>();
builder.Services.AddSingleton<IDetectorCuotasDesfasadas, DetectorCuotasDesfasadas>();

builder.Services.AddControllers(o => o.Filters.Add<OddsApiExceptionFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Services.GetRequiredService<IOptions<OddsApiOptions>>().Value.ModoDemo)
{
    app.Logger.LogWarning("Modo demo activo: se sirven partidos de ejemplo y no se consulta The Odds API");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(PoliticaCorsWeb);

app.UseAuthorization();

app.MapControllers();

app.Run();
