using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using vendtechext.BLL.Common;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.Controllers.Base;
using vendtechext.Hangfire;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("account/v1")]
    public class AccountController : BaseController
    {
        private readonly IB2bAccountService service;
        private readonly IJobService _jobService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public AccountController(ILogger<BaseController> logger, IB2bAccountService b2bAccountService, IJobService jobService, IBackgroundJobClient backgroundJobClient) : base(logger)
        {
            service = b2bAccountService;
            _jobService = jobService;
            _backgroundJobClient = backgroundJobClient;
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

        [HttpGet("qu-account")]
        public IActionResult UpdateBusinessUser2()
        {
            _backgroundJobClient.Enqueue(() => _jobService.FireAndForegtJob());
            return Ok();
        }
    }
}