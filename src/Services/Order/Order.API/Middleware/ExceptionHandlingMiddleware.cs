using Order.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace Order.API.Middleware;

public class ExceptionHandlingMiddleware
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var response = new ErrorResponse
        {
            Message = "An error occurred while processing your request."
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                response.Message = "One or more validation errors occurred.";
                response.Errors = validationException.Errors;
                _logger.LogWarning(
                    exception,
                    "Validation error occurred. Errors: {ValidationErrors}",
                    JsonSerializer.Serialize(validationException.Errors));
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                response.Message = notFoundException.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", notFoundException.Message);
                break;

            case InvalidOperationException invalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                response.Message = invalidOperationException.Message;
                _logger.LogWarning(
                    exception,
                    "Invalid operation: {Message}",
                    invalidOperationException.Message);
                break;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception occurred. Type: {ExceptionType}, Message: {Message}",
                    exception.GetType().Name,
                    exception.Message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}
