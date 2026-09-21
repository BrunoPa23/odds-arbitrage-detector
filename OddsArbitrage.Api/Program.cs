using Microsoft.Extensions.Options;
using OddsArbitrage.Api.Services.OddsApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddOptions<OddsApiOptions>()
    .Bind(builder.Configuration.GetSection(OddsApiOptions.Seccion))
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
builder.Services.AddScoped<IOddsApiService, OddsApiServiceConCache>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
