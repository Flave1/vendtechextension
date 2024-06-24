
namespace vendtechext.BLL.Middlewares
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using vendtechext.BLL.Interfaces;

    public class EndpointValidatorAttribute : Attribute, IAuthorizationFilter
    { 

        public bool AllowMultiple => false;
         

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var credentialService = context.HttpContext.RequestServices.GetRequiredService<IB2bAccountService>();

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Client", out var extractedClient))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var apiKey = extractedApiKey.First();
            var clientKey = extractedClient.First();

            if (!credentialService.ValidateUser(clientKey, apiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}
