using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("admin-integrator/v1")]
    [Authorize(Roles = "Super Admin")]
    public class AdminIntegratorController : BaseController
    {
        private readonly IIntegratorService service;
        public AdminIntegratorController(ILogger<BaseController> logger, IIntegratorService b2bAccountService) : base(logger)
        {
            service = b2bAccountService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBusinessUser([FromBody] BusinessUserCommandDTO businessUser)
        {
            var result = await service.CreateBusinessAccount(businessUser);
            return Ok(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateBusinessUser([FromBody] BusinessUserDTO businessUser)
        {
            var result = await service.UpdateBusinessAccount(businessUser);
            return Ok(result);
        }

        [HttpPost("get")]
        public async Task<IActionResult> GetBusinessUsers(PaginatedSearchRequest request)
        {
            var result = await service.GetIntegrators(request);
            return Ok(result);
        }

        [HttpGet("single")]
        public async Task<IActionResult> GetBusinessUser(string id)
        {
            Guid integratorid = Guid.Parse(id);
            var result = await service.GetIntegrator(integratorid);
            return Ok(result);
        }


        [HttpPost("enable-disable")]
        public async Task<IActionResult> EnableDisable([FromBody] EnableIntegrator businessUser)
        {
            var result = await service.EnableDisable(businessUser);
            return Ok(result);
        }

    }
}