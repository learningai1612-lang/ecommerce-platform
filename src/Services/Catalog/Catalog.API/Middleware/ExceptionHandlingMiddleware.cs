using Catalog.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace Catalog.API.Middleware;

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
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Error = exception.Message,
            Details = new List<string>()
        };

        switch (exception)
        {
            case Application.Common.Exceptions.ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error = "Validation failed";
                errorResponse.Details = validationException.Errors
                    .SelectMany(e => e.Value.Select(v => $"{e.Key}: {v}"))
                    .ToList();
                _logger.LogWarning(exception, "Validation error occurred");
                break;

            case NotFoundException notFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Error = notFoundException.Message;
                _logger.LogWarning(exception, "Resource not found");
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Error = "Unauthorized access";
                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = "An error occurred while processing your request";
                errorResponse.Details = new List<string> { exception.Message };
                _logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
}
