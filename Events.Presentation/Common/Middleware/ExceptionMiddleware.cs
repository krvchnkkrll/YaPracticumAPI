using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Events.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Events.Presentation.Common.Middleware;

internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception");

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = exception switch
        {
            KeyNotFoundException => new ProblemDetails
            {
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = exception.Message
            },

            ArgumentException or ValidationException => new ProblemDetails
            {
                Title = "Invalid request",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception.Message
            },
            
            NoAvailableSeatsException => new ProblemDetails
            {
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = exception.Message
            },
            
            EventAlreadyStartedException => new ProblemDetails
            {
                Title = "BadRequest",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception.Message,
            },

            _ => new ProblemDetails
            {
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.Message
            }
        };
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problem.Status ?? (int) HttpStatusCode.InternalServerError;

        var json = JsonSerializer.Serialize(problem);

        await context.Response.WriteAsync(json);
    }
}