﻿
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

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
                context.Result = new UnauthorizedResult();
                return;
            }

          
            var apiKey = extractedApiKey.First();
            var integrator = credentialService.GetIntegratorIdAndName(apiKey).Result;

            if (integrator == ("404", "not_found"))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            if(integrator == ("403", "forbidden"))
            {
                APIResponse response = new Response().WithStatus("failed")
                   .WithStatusCode(403)
                   .WithMessage("API Vending is Disabled")
                   .WithDetail("API Vending is Disabled")
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
