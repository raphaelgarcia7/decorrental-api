namespace DecorRental.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemName = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveIncomingCorrelationId(context);
        context.TraceIdentifier = correlationId;
        context.Items[ItemName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }

    public static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemName, out var correlationIdValue) &&
            correlationIdValue is string correlationId &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        return context.TraceIdentifier;
    }

    private static string ResolveIncomingCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            var headerValue = headerValues.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}
