using DecorRental.Application.Exceptions;
using DecorRental.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "not_found", exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "conflict", exception.Message);
        }
        catch (DomainException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "domain_error", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing request");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "internal_error",
                "An unexpected error happened while processing the request.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string code,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();

        var correlationId = CorrelationIdMiddleware.ResolveCorrelationId(context);
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetTitle(statusCode),
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };
        problemDetails.Extensions["code"] = code;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = correlationId;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string GetTitle(int statusCode)
        => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };
}
