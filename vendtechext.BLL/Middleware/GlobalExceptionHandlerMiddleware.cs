using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using vendtechext.BLL.Exceptions;
using vendtechext.DAL.Common;
using vendtechext.Helper;
using vendtechext.Contracts;
namespace vendtechext.BLL.Middleware
{
    public class GlobalExceptionHandlerMiddleware: BaseService
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (BadRequestException ex)
            {
                _logger.LogError(ex, "Bad Request error.");
                await HandleExceptionAsync(httpContext, ex, ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error.");
                await HandleExceptionAsync(httpContext, ex, "Invalid JSON format.");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: input is null.");
                await HandleExceptionAsync(httpContext, ex, "request body is null.");
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: unsupported type.");
                await HandleExceptionAsync(httpContext, ex, "Unsupported type for deserialization.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: invalid operation.");
                await HandleExceptionAsync(httpContext, ex, "Invalid operation during deserialization.");
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleExceptionAsync(httpContext, ex, "Invalid operation during deserialization.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(httpContext, ex, "Internal Server Error from the middleware server.");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string message = null)
        {

            var _log = context.RequestServices.GetRequiredService<LogService>();
            

            context.Response.ContentType = "application/json";
            if (exception is BadRequestException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else if(exception is UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                context.Response.StatusCode = 200;
            }

            APIResponse response = Response
                .WithStatus("failed")
                .WithStatusCode(context.Response.StatusCode)
                .WithMessage(message)
                .WithDetail(exception.Message ?? "")
                .GenerateResponse();

            var jsonResponse = JsonConvert.SerializeObject(response);

            if(context.Response.StatusCode != 400)
                _log.Log(LogType.Error, context.Response.StatusCode.ToString(), jsonResponse, exception.StackTrace);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
