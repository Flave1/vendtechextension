using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.DTO;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("edsa/v2/purchase")]
    public class RtsElectricitySalesController : ControllerBase
    {
        private readonly ILogger<RtsElectricitySalesController> _logger;

        public RtsElectricitySalesController(ILogger<RtsElectricitySalesController> logger)
        {
            _logger = logger;
        }


        [HttpPost("", Name = "")]
        public IActionResult purchase([FromBody] RTSRequestmodel request)
        {

            //if (HttpContext.Items.TryGetValue("RequestModel", out var requestModelObj) && requestModelObj is RequestModel requestModel)
            //{
            //    // Now you have the requestModel populated with JSON parameters
            //    // Do something with requestModel
            //    return Ok(requestModel);
            //}
            //else
            //{
            //    return BadRequest("Invalid request model");
            //}


            _logger.LogInformation(1, null, "This is it");
            return Ok(request);
        }
    }
}
