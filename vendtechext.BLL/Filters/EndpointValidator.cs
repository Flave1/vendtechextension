
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Helper;

namespace vendtechext.BLL.Middlewares
{

    public class EndpointValidatorAttribute : Attribute, IAuthorizationFilter
    {

        public bool AllowMultiple => false;


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the action or controller has the [AllowAnonymous] attribute
            var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

            // If [AllowAnonymous] is present, skip authorization
            if (hasAllowAnonymous)
            {
                return;
            }

            // Perform your authorization logic as usual
            var credentialService = context.HttpContext.RequestServices.GetRequiredService<IIntegratorService>();

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
            {
                ExecutionResult executionResult = new BaseService().GenerateExecutionResult(new UnauthorizedAccessException("Unauthorized Access"), API_MESSAGE_CONSTANCE.AUTHENTICATION_ERROR);
                APIResponse response = new Response().WithStatus("failed")
                   .WithMessage("Unauthorized Access")
                   .WithDetail("Credentials are required in other to vend")
                   .WithType(executionResult)
                   .GenerateResponse();

                var jsonResponse = JsonConvert.SerializeObject(response);

                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = jsonResponse,
                    ContentType = "application/json"
                };
                return;
            }

          
            var apiKey = extractedApiKey.First();
            var integrator = credentialService.GetIntegratorIdAndName(apiKey).Result;

            if (integrator == ("404", "not_found"))
            {
                ExecutionResult executionResult = new BaseService().GenerateExecutionResult(new UnauthorizedAccessException("Access credentials not valid"), API_MESSAGE_CONSTANCE.NOTFOUND_ERROR);
                APIResponse response = new Response().WithStatus("failed")
                   .WithMessage("Access credentials not valid")
                   .WithDetail("Valid credentials are required in other to vend")
                   .WithType(executionResult)
                   .GenerateResponse();

                var jsonResponse = JsonConvert.SerializeObject(response);

                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = jsonResponse,
                    ContentType = "application/json"
                };
                return;
            }
            if(integrator == ("403", "forbidden"))
            {

                ExecutionResult executionResult = new BaseService().GenerateExecutionResult(new ForbiddenResultException("Access to API denied"), API_MESSAGE_CONSTANCE.ACCESS_DENIED);
                APIResponse response = new Response().WithStatus("failed")
                   .WithMessage("Access to API denied")
                   .WithDetail("This usually occurs when Vendor account is disabled")
                   .WithType(executionResult)
                   .GenerateResponse();

                var jsonResponse = JsonConvert.SerializeObject(response);

                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = jsonResponse,
                    ContentType = "application/json"
                };
                return;
            }
            //Pass ClientKey to controller
            context.HttpContext.Items["IntegratorId"] = integrator.Item1;
            context.HttpContext.Items["IntegratorName"] = integrator.Item2;
        }

    }
}
