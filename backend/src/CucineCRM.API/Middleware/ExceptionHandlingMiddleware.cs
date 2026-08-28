using System.Net;
using System.Text.Json;
using CucineCRM.Application.Common;

namespace CucineCRM.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore non gestito durante l'elaborazione della richiesta {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            AuthenticationException => (HttpStatusCode.Unauthorized, "Errore di autenticazione"),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Accesso negato"),
            NotFoundException => (HttpStatusCode.NotFound, "Risorsa non trovata"),
            ValidationAppException => (HttpStatusCode.BadRequest, "Errore di validazione"),
            _ => (HttpStatusCode.InternalServerError, "Errore interno del server")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Per le eccezioni applicative tipizzate il messaggio è pensato per l'utente finale
        // (es. "Email o password non corretti."). Per tutto il resto (500 non previsti: bug,
        // eccezioni EF Core/Npgsql, NullReferenceException...) non va mai esposto exception.Message
        // al client: potrebbe rivelare dettagli interni (connection string, path, stack). Il
        // dettaglio completo resta comunque nei log tramite _logger.LogError sopra.
        var detail = statusCode == HttpStatusCode.InternalServerError
            ? "Si è verificato un errore imprevisto. Riprovare più tardi o contattare l'assistenza."
            : exception.Message;

        var payload = JsonSerializer.Serialize(new
        {
            title,
            status = (int)statusCode,
            detail
        });

        return context.Response.WriteAsync(payload);
    }
}
