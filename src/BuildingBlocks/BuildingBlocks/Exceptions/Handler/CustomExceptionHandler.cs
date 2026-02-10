using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions.Application;
using BuildingBlocks.Exceptions.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler;

public sealed class CustomExceptionHandler
    (ILogger<CustomExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Proper structured logging with full exception info
        logger.LogError(exception,
            "Unhandled exception occurred at {Time}",
            DateTime.UtcNow);

        var (detail, title, statusCode) = exception switch
        {
            DomainException =>
                (exception.Message, "DomainError", StatusCodes.Status400BadRequest),

            ExternalServiceException =>
                (
                    exception.Message,
                    exception.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status502BadGateway
                ),

            ValidationException validationException =>
                (validationException.Message, "ValidationError", StatusCodes.Status400BadRequest),

            ForbiddenException =>
                (exception.Message, "Forbidden", StatusCodes.Status403Forbidden),

            NotFoundException =>
                (exception.Message, "NotFound", StatusCodes.Status404NotFound),

            _ =>
                ("An unexpected error occurred",
                 "InternalServerError",
                 StatusCodes.Status500InternalServerError)
        };

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };

        // Correlation / Trace id
        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

        // Attach validation errors if present
        if (exception is ValidationException ve)
        {
            problemDetails.Extensions.Add("validationErrors", ve.Errors);
        }

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}