using System.ComponentModel.DataAnnotations;
using OddsArbitrage.Api.Services.OddsApi;

namespace OddsArbitrage.Tests.Services.OddsApi;

public class OddsApiOptionsTests
{
    [Fact]
    public void MercadosAConsultar_SinConfigurar_DevuelveSoloH2H()
    {
        // Arrange
        var opciones = new OddsApiOptions();

        // Act y Assert
        Assert.Equal([MercadosOddsApi.H2H], opciones.MercadosAConsultar());
    }

    [Fact]
    public void MercadosAConsultar_NormalizaYQuitaRepetidos()
    {
        // Arrange
        var opciones = new OddsApiOptions { Mercados = ["h2h", " Totals ", "H2H", ""] };

        // Act y Assert
        Assert.Equal([MercadosOddsApi.H2H, MercadosOddsApi.Totals], opciones.MercadosAConsultar());
    }

    [Fact]
    public void Validar_ConMercadoNoSoportado_DevuelveError()
    {
        // Arrange
        var opciones = new OddsApiOptions { ApiKey = "clave-falsa", Mercados = ["h2h", "outrights"] };
        var resultados = new List<ValidationResult>();

        // Act
        var esValido = Validator.TryValidateObject(opciones, new ValidationContext(opciones), resultados, validateAllProperties: true);

        // Assert
        Assert.False(esValido);
        var error = Assert.Single(resultados);
        Assert.Contains("outrights", error.ErrorMessage);
    }

    [Fact]
    public void Validar_ConMercadosSoportados_EsValido()
    {
        // Arrange
        var opciones = new OddsApiOptions { ApiKey = "clave-falsa", Mercados = ["h2h", "totals", "spreads"] };

        // Act
        var esValido = Validator.TryValidateObject(opciones, new ValidationContext(opciones), [], validateAllProperties: true);

        // Assert
        Assert.True(esValido);
    }

    [Fact]
    public void Validar_SinApiKeyFueraDelModoDemo_DevuelveError()
    {
        // Arrange
        var opciones = new OddsApiOptions();
        var resultados = new List<ValidationResult>();

        // Act
        var esValido = Validator.TryValidateObject(opciones, new ValidationContext(opciones), resultados, validateAllProperties: true);

        // Assert
        Assert.False(esValido);
        var error = Assert.Single(resultados);
        Assert.Contains(nameof(OddsApiOptions.ApiKey), error.MemberNames);
    }

    [Fact]
    public void Validar_SinApiKeyEnModoDemo_EsValido()
    {
        // Arrange
        var opciones = new OddsApiOptions { ModoDemo = true };

        // Act
        var esValido = Validator.TryValidateObject(opciones, new ValidationContext(opciones), [], validateAllProperties: true);

        // Assert
        Assert.True(esValido);
    }
}
