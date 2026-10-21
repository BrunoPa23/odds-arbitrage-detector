using OddsArbitrage.Api.Domain;

namespace OddsArbitrage.Api.Services.ValueBets;

public interface IDetectorCuotasDesfasadas
{
    // Ventaja esperada minima, en porcentaje, para considerar desfasada una cuota
    const decimal VentajaMinimaPorDefecto = 3m;

    // Compara cada cuota con el consenso sin margen de su mercado y linea (o solo del mercado pedido)
    // y devuelve las que tienen una ventaja mayor o igual a ventajaMinima, ordenadas por ventaja descendente
    IReadOnlyList<CuotaDesfasada> Detectar(
        IEnumerable<Partido> partidos,
        TipoMercado? mercado = null,
        decimal ventajaMinima = VentajaMinimaPorDefecto,
        int minimoCasas = ProbabilidadJusta.MinimoCasasPorDefecto);
}
