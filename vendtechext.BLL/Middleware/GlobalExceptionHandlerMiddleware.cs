using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using vendtechext.BLL.Common;
using vendtechext.BLL.Interfaces;
namespace vendtechext.BLL.Middleware
{
    public class GlobalExceptionHandlerMiddleware
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

                //httpContext.Request.EnableBuffering();
                //var requestBodyStream = new StreamReader(httpContext.Request.Body);
                //var requestBody = await requestBodyStream.ReadToEndAsync();
         
                //httpContext.Request.Body.Position = 0;

                //var requestModel = JsonSerializer.Deserialize<RTSRequestmodel>(requestBody, new JsonSerializerOptions
                //{
                //    PropertyNameCaseInsensitive = true
                //});

                //httpContext.Items["RequestModel"] = requestModel;



                await _next(httpContext);

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
                var errorlogService = httpContext.RequestServices.GetRequiredService<IErrorlogService>();
                errorlogService.LogExceptionToDatabase(ex, clientKey);
                HandleExceptionAsync(httpContext, ex, "Internal Server Error from the middleware server.");
            }
        }

        private void HandleExceptionAsync(HttpContext context, Exception exception, string message = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = Response.Instance
                    .WithStatus("failed")
                    .WithStatusCode(context.Response.StatusCode)
                    .WithMessage(message)
                    .WithDetail(exception.Message)
                    .GenerateResponse();

            var jsonResponse = JsonConvert.SerializeObject(response);

            context.Response.WriteAsync(jsonResponse);
        }
    }
}
