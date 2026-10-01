namespace PruebaTecnica.Application.Common.Security;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-KEY";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key requerida en header 'X-API-KEY'.");
            return;
        }

        var configuredApiKey = configuration.GetValue<string>("ApiKey");

        if (string.IsNullOrWhiteSpace(configuredApiKey) || !configuredApiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key invalida.");
            return;
        }

        await _next(context);
    }
}