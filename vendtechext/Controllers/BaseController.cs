using Microsoft.AspNetCore.Mvc;

namespace vendtechext.Controllers
{
    public class BaseController : ControllerBase
    {
        public readonly ILogger<BaseController> _logger;

        public BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }
        protected IActionResult? ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return null;
        }

    }
}