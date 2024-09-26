using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.Controllers.Base;

namespace vendtechext.Controllers
{
    //[ApiController]
    //[Route("account/v1")]
    //public class JobController : BaseController
    //{
    //    private readonly IB2bAccountService service;               
    //    public JobController(ILogger<BaseController> logger, IB2bAccountService b2bAccountService) : base(logger)
    //    {
    //        service = b2bAccountService;
    //    }


    //    [HttpPost("create-account")]
    //    public async Task<IActionResult> CreateBusinessUser([FromBody] IntegratorCommandDTO businessUser)
    //    {
    //        await service.CreateBusinessAccount(businessUser);
    //        return Ok(businessUser);
    //    }

    //    [HttpPost("update-account")]
    //    public async Task<IActionResult> UpdateBusinessUser([FromBody] IntegratorCommandDTO businessUser)
    //    {
    //        await service.UpdateBusinessAccount(businessUser);
    //        return Ok(businessUser);
    //    }
    //}
}