using Microsoft.AspNetCore.Mvc;

namespace MedReability.Api.Common;

public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing request.");

            var (statusCode, title, detail) = MapException(ex);

            var problem = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = detail,
                Type = "about:blank"
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception ex)
    {
        return ex switch
        {
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                ex.Message),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Not found",
                ex.Message),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                ex.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred.")
        };
    }
}
