using System.Net;
using System.Text.Json;
using FluentValidation;
using OrderFlow.CQRS.Domain.Exceptions;

namespace OrderFlow.CQRS.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Code = "VALIDATION_ERROR";
                errorResponse.Message = "Doğrulama hatası oluştu.";
                errorResponse.Errors = validationException.Errors
                    .Select(e => new ErrorDetail(e.PropertyName, e.ErrorMessage))
                    .ToList();
                _logger.LogWarning(exception, "Validation error occurred: {@Errors}", errorResponse.Errors);
                break;

            case NotFoundException notFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Code = notFoundException.Code;
                errorResponse.Message = notFoundException.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", notFoundException.Message);
                break;

            case DomainException domainException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Code = domainException.Code;
                errorResponse.Message = domainException.Message;
                _logger.LogWarning(exception, "Domain error: {Message}", domainException.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Code = "INTERNAL_ERROR";
                errorResponse.Message = "Beklenmeyen bir hata oluştu.";
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                break;
        }

        errorResponse.TraceId = context.TraceIdentifier;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await response.WriteAsJsonAsync(errorResponse, options);
    }
}

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public List<ErrorDetail>? Errors { get; set; }
}

public record ErrorDetail(string Field, string Message);
