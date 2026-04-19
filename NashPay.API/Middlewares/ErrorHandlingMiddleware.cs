using System.Net;
using System.Text.Json;
using NashPay.API.DTOs;

namespace NashPay.API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NashPay Error: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponseDto { Success = false };

            // Status Code Setting
            switch (exception)
            {
                case ArgumentException:
                case InvalidOperationException: // Added for business logic errors
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access - Please login again.";
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "The requested resource was not found.";
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    // Production pe detail hide rakhein, Dev pe show karein
                    response.Message = _env.IsDevelopment() 
                        ? $"Internal Server Error: {exception.Message}" 
                        : "Something went wrong on our end. Please try again later.";
                    break;
            }

            // Standard JSON options
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var result = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(result);
        }
    }
}