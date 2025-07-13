using System.Net;
using System.Text.Json;
using YopoAPI.DTOs;

namespace YopoAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            string message;
            string status;
            List<string> errors = new List<string>();

            switch (exception)
            {
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    message = "Invalid request";
                    status = "BAD_REQUEST";
                    errors.Add(exception.Message);
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    message = "Unauthorized access";
                    status = "UNAUTHORIZED";
                    errors.Add(exception.Message);
                    break;
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    message = "Resource not found";
                    status = "NOT_FOUND";
                    errors.Add(exception.Message);
                    break;
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    message = "Invalid operation";
                    status = "INVALID_OPERATION";
                    errors.Add(exception.Message);
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    message = "An error occurred while processing your request";
                    status = "INTERNAL_SERVER_ERROR";
                    errors.Add(exception.Message);
                    break;
            }

            var errorResponse = ApiResponse.Error(message, errors, status);
            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await response.WriteAsync(result);
        }
    }

}

