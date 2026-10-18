namespace OddsArbitrage.Api.Domain;

// Resultado al que apuesta una cuota; los valores validos dependen del mercado
public enum ResultadoCuota
{
    Local,
    Empate,
    Visitante,

    // Mas y Menos solo aplican al mercado de totales
    Mas,
    Menos
}
