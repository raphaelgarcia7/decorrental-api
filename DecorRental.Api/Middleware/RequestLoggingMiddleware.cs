using System.Diagnostics;

namespace DecorRental.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var correlationId = CorrelationIdMiddleware.ResolveCorrelationId(context);

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms (CorrelationId: {CorrelationId})",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}
