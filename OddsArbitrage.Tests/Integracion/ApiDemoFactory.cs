using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Tests.Integracion;

// Levanta la API en memoria en modo demo: los endpoints leen el fixture y nunca salen a la red
public sealed class ApiDemoFactory : WebApplicationFactory<Program>
{
    public static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Un ambiente propio evita cargar user secrets (y con ellos una API key real)
        builder.UseEnvironment("Testing");
        builder.UseSetting("TheOddsApi:ModoDemo", "true");
        builder.UseSetting("TheOddsApi:ApiKey", string.Empty);
        builder.UseSetting("TheOddsApi:BaseUrl", "https://api.invalida.test/");

        // Red de seguridad: si algo llegara a usar el cliente real, falla en lugar de llamar a internet
        builder.ConfigureServices(services =>
            services.AddHttpClient<OddsApiService>()
                .ConfigurePrimaryHttpMessageHandler(() => new HandlerSinRed()));
    }

    public async Task<T> GetJsonAsync<T>(string url)
    {
        using var cliente = CreateClient();
        var respuesta = await cliente.GetAsync(url);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<T>(OpcionesJson))!;
    }

    private sealed class HandlerSinRed : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new InvalidOperationException($"Los tests de integracion no deben llamar a {request.RequestUri}");
    }
}
