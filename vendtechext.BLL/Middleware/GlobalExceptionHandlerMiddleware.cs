using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Services;
namespace vendtechext.BLL.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IErrorlogService _errorlog;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IErrorlogService errorlog)
        {
            _next = next;
            _logger = logger;
            _errorlog = errorlog;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                httpContext.Request.Headers.TryGetValue("X-Client", out var clientKey);
                _errorlog.LogExceptionToDatabase(ex, clientKey);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;


            var response = new APIResponse
            {
                Status = "failed",
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error from the middleware server.",
                Detailed = exception.Message // Don't expose exception details in production
            };
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
