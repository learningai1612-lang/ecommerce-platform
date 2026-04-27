using Inventory.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Inventory.API.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response = exception switch
        {
            ValidationException validationException => new
            {
                type = "ValidationException",
                message = "One or more validation errors occurred",
                details = validationException.Errors.Select(e => new
                {
                    propertyName = e.Key,
                    errorMessages = e.Value
                }).ToList(),
                statusCode = StatusCodes.Status400BadRequest
            },
            NotFoundException notFoundException => new
            {
                type = "NotFoundException",
                message = notFoundException.Message,
                details = Array.Empty<object>(),
                statusCode = StatusCodes.Status404NotFound
            },
            InvalidOperationException invalidOpException => new
            {
                type = "InvalidOperationException",
                message = invalidOpException.Message,
                details = Array.Empty<object>(),
                statusCode = StatusCodes.Status400BadRequest
            },
            DbUpdateConcurrencyException concurrencyException => new
            {
                type = "ConcurrencyException",
                message = "A concurrency conflict occurred. The item may have been modified by another user. Please retry.",
                details = Array.Empty<object>(),
                statusCode = StatusCodes.Status409Conflict
            },
            _ => new
            {
                type = "InternalServerError",
                message = "An unexpected error occurred. Please try again later.",
                details = Array.Empty<object>(),
                statusCode = StatusCodes.Status500InternalServerError
            }
        };

        var statusCode = (int)response.GetType().GetProperty("statusCode")!.GetValue(response)!;
        var type = (string)response.GetType().GetProperty("type")!.GetValue(response)!;
        var message = (string)response.GetType().GetProperty("message")!.GetValue(response)!;
        var details = response.GetType().GetProperty("details")!.GetValue(response)!;

        context.Response.StatusCode = statusCode;

        _logger.LogError(exception,
            "Exception occurred: {ExceptionType} - {Message}",
            exception.GetType().Name,
            exception.Message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            type,
            message,
            details
        }));
    }
}
