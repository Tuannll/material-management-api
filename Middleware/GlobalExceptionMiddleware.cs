using System.Net;
using System.Text.Json;
using TH4.Exceptions;
using TH4.Models.Common;

namespace TH4.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var statusCode = exception switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            UserFriendlyException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        ApiResponse<object> response;

        if (exception is UserFriendlyException userFriendlyException)
        {
            response = ApiResponse<object>.ErrorResponse(userFriendlyException.ErrorCode, userFriendlyException.Message, statusCode);
        }
        else if (exception is NotFoundException notFoundException)
        {
            response = ApiResponse<object>.ErrorResponse(ErrorCodes.MaterialNotFound, notFoundException.Message, statusCode);
        }
        else
        {
            response = ApiResponse<object>.FailureResponse("An unexpected error occurred.", [exception.Message], statusCode);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
