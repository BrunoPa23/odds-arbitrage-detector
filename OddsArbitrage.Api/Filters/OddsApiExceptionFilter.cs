using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OddsArbitrage.Api.Filters;

// Traduce los fallos al consultar The Odds API en respuestas HTTP coherentes para todos los endpoints
public sealed class OddsApiExceptionFilter(ILogger<OddsApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case HttpRequestException ex:
                logger.LogError(ex, "The Odds API respondio con error ({StatusCode})", ex.StatusCode);
                context.Result = Problema(StatusCodes.Status502BadGateway, "No se pudieron obtener las cuotas de The Odds API");
                break;

            case TaskCanceledException when !context.HttpContext.RequestAborted.IsCancellationRequested:
                logger.LogError("The Odds API no respondio a tiempo");
                context.Result = Problema(StatusCodes.Status504GatewayTimeout, "The Odds API no respondio a tiempo");
                break;

            default:
                return;
        }

        context.ExceptionHandled = true;
    }

    private static ObjectResult Problema(int statusCode, string titulo)
        => new(new ProblemDetails { Status = statusCode, Title = titulo }) { StatusCode = statusCode };
}
