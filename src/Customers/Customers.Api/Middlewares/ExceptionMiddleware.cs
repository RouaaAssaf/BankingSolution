using System.Text.Json;
using Customers.Domain.Exceptions;


public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException dex)
        {
            _logger.LogWarning(dex, dex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = dex.StatusCode;

            var apiError = new ApiError(dex.StatusCode, dex.Message);
            var json = JsonSerializer.Serialize(apiError, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var message = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.";
            var apiError = new ApiError(context.Response.StatusCode, message);
            var json = JsonSerializer.Serialize(apiError, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(json);
        }
    }
}
