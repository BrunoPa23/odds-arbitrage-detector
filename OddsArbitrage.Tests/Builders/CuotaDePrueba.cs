using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Tests.Builders;

// Helper para construir una Cuota suelta (sin necesidad de un Partido completo),
// util para probar ProbabilidadImplicita y RepartidorStake de forma aislada.
public static class CuotaDePrueba
{
    public static Cuota Crear(ResultadoPartido resultado, decimal valor, string casaClave = "casa-1")
        => new()
        {
            Casa = new CasaDeApuestas { Clave = casaClave, Nombre = casaClave },
            Resultado = resultado,
            Valor = valor,
            UltimaActualizacion = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
}
