using System.Net;
using System.Text.Json;
using MeetingRoomManagement.Api.Domain.Exceptions;
using MeetingRoomManagement.Api.DTOs;

namespace MeetingRoomManagement.Api.Middleware;

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
        var statusCode = HttpStatusCode.InternalServerError;
        var code = "INTERNAL_SERVER_ERROR";
        var message = "An unexpected error occurred.";

        switch (exception)
        {
            case BookingValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                code = valEx.Code;
                message = valEx.Message;
                _logger.LogWarning("Validation error ({Code}): {Message}", code, message);
                break;

            case BookingConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                code = conflictEx.Code;
                message = conflictEx.Message;
                _logger.LogWarning("Booking conflict ({Code}): {Message}", code, message);
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                code = notFoundEx.Code;
                message = notFoundEx.Message;
                _logger.LogWarning("Resource not found ({Code}): {Message}", code, message);
                break;

            case BadHttpRequestException badRequestEx:
                statusCode = HttpStatusCode.BadRequest;
                code = "BAD_REQUEST";
                message = badRequestEx.Message;
                _logger.LogWarning(badRequestEx, "Bad HTTP request");
                break;

            default:
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse(message, code);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
