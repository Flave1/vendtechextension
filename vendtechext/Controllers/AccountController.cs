using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using vendtechext.BLL.Common;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("account/v1")]
    public class AccountController : BaseController
    {
        private readonly IB2bAccountService service;               
        public AccountController(ILogger<BaseController> logger, IB2bAccountService b2bAccountService) : base(logger)
        {
            service = b2bAccountService;
        }


        [HttpPost("create-account")]
        public async Task<IActionResult> CreateBusinessUser([FromBody] BusinessUserCommandDTO businessUser)
        {
            await service.CreateBusinessAccount(businessUser);
            return Ok(businessUser);
        }

        [HttpPost("update-account")]
        public async Task<IActionResult> UpdateBusinessUser([FromBody] BusinessUserCommandDTO businessUser)
        {
            await service.UpdateBusinessAccount(businessUser);
            return Ok(businessUser);
        }
    }
}