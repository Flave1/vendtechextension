using Microsoft.AspNetCore.Http;

namespace vendtechext.BLL.Filters
{
    public class JwtAuthorizationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(token) && !request.Headers.Contains("Authorization"))
            {
                request.Headers.Add("Authorization", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

}
