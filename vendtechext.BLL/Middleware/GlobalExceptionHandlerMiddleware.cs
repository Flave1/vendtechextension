using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using vendtechext.BLL.Common;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Exceptions;
using vendtechext.DAL.Common;
namespace vendtechext.BLL.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly ILogService _log;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, ILogService log)
        {
            _next = next;
            _logger = logger;
            _log = log;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (BadRequestException ex)
            {
                _logger.LogError(ex, "Bad request error.");
                HandleExceptionAsync(httpContext, ex, ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error.");
                HandleExceptionAsync(httpContext, ex, "Invalid JSON format.");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: input is null.");
                HandleExceptionAsync(httpContext, ex, "Request body is null.");
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: unsupported type.");
                HandleExceptionAsync(httpContext, ex, "Unsupported type for deserialization.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: invalid operation.");
                HandleExceptionAsync(httpContext, ex, "Invalid operation during deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                httpContext.Request.Headers.TryGetValue("X-Client", out var clientKey);
                var errorlogService = httpContext.RequestServices.GetRequiredService<ILogService>();
                HandleExceptionAsync(httpContext, ex, "Internal Server Error from the middleware server.");
            }
        }

        private void HandleExceptionAsync(HttpContext context, Exception exception, string message = null)
        {

            context.Response.ContentType = "application/json";
            if (exception is BadRequestException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            APIResponse response = Response.Instance
                .WithStatus("failed")
                .WithStatusCode(context.Response.StatusCode)
                .WithMessage(message)
                .WithDetail(exception.Message)
                .GenerateResponse();

            var jsonResponse = JsonConvert.SerializeObject(response);
            _log.Log(LogType.Error, context.Response.StatusCode.ToString(), jsonResponse);
            context.Response.WriteAsync(jsonResponse);
        }
    }
}
