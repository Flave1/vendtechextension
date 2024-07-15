using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using vendtechext.BLL.Common;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
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

                string logs = $"certificate: {httpContext.Connection.ClientCertificate} \n" +
                 $"id: {httpContext.Connection.Id} \n" +
                 $"RemoteIpAddress: {httpContext.Connection.RemoteIpAddress} \n" +
                 $"RemotePort: {httpContext.Connection.RemotePort} \n" +
                 $"LocalIpAddress: {httpContext.Connection.LocalIpAddress} \n" +
                 $"RemotePort: {httpContext.Connection.RemotePort} \n" +
                 $"LocalPort: {httpContext.Connection.LocalPort} \n" +
                 $"Headers: {httpContext.Request.Headers}  \n" +
                $"Path: {httpContext.Request.Path} \n" +
                $"Host: {httpContext.Request.Host}  \n" +
                $"Cookies: {httpContext.Request.Cookies} \n" +
                $"IsHttps: {httpContext.Request.IsHttps} \n" +
                $"Protocol: {httpContext.Request.Protocol} \n" +
                //$"Features: {httpContext.Features}  \n" +
                $"Response: {httpContext.Response} \n" +
                $"LocalPort: {httpContext.Connection.LocalPort} \n";
               //$"Items: {httpContext.Items}" 

               _errorlog.LogExceptionToDatabase(new Exception(logs), "connection");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error.");
                await HandleExceptionAsync(httpContext, ex, "Invalid JSON format.");
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: input is null.");
                await HandleExceptionAsync(httpContext, ex, "Request body is null.");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                httpContext.Request.Headers.TryGetValue("X-Client", out var clientKey);
                _errorlog.LogExceptionToDatabase(ex, clientKey);
                await HandleExceptionAsync(httpContext, ex, "Internal Server Error from the middleware server.");
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, string message = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = Response.Instance
                    .WithStatus("failed")
                    .WithStatusCode(context.Response.StatusCode)
                    .WithMessage(message)
                    .WithDetail(exception.Message)
                    .GenerateResponse();
             
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
