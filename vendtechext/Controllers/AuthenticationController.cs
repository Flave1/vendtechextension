using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Middlewares;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("auth/v2/user")]
    [EndpointValidator]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }

        [HttpPost("validate", Name = "validate")]
        public IActionResult Validate([FromBody] MessageBody request)
        {
            _logger.LogInformation(1, null, "This is it");
            return Ok(request);
        }

        [HttpGet("exception")]
        public IActionResult ThrowException()
        {
            throw new Exception("Test exception");
        }

    }
}