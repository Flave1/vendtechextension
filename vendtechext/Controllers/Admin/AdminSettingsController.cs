using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendtechext.Contracts;
using vendtechext.Helper;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("admin-settings/v1/")]
    [Authorize]
    public class AdminSettingsController : ControllerBase
    {
        private readonly AppConfiguration config;

        public AdminSettingsController(AppConfiguration config)
        {
            this.config = config;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SettingDto request)
        {
            await config.SaveSettings(request);
            return Ok(request);
        }
    }
}