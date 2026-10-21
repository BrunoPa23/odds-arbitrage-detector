using System.ComponentModel.DataAnnotations;

namespace OddsArbitrage.Api.Services.ValueBets;

public sealed class ValueBetsOptions
{
    public const string Seccion = "ValueBets";

    // Umbral por defecto de GET /api/value-bets cuando no se envia ventajaMinima
    [Range(0, 100)]
    public decimal VentajaMinimaPorcentaje { get; set; } = IDetectorCuotasDesfasadas.VentajaMinimaPorDefecto;

    // Casas necesarias para que el consenso sin margen sea confiable
    [Range(1, 50)]
    public int MinimoCasas { get; set; } = ProbabilidadJusta.MinimoCasasPorDefecto;
}
