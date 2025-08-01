using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("bank/v1/")]
    public class BankController : ControllerBase
    {
        private readonly IBankService _bankService;
        public BankController(IBankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var banks = await _bankService.GetAllBanksAsync();

            return Ok(banks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bank = await _bankService.GetBankByIdAsync(id);
            if (bank == null) return NotFound();
            return Ok(bank);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBankRequest request)
        {
            var bank = await _bankService.CreateBankAsync(request);
            return Ok(bank);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBankRequest request)
        {
            request.Id = id;
            var bank = await _bankService.UpdateBankAsync(request);
            if (bank == null) return NotFound();
            return Ok(bank);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _bankService.DeleteBankAsync(id);
            return Ok(deleted);
        }

        [HttpGet("get-banks-select")]
        public async Task<IActionResult> GetBanksSelect()
        {
            var result = await _bankService.GetBankSelect();
            return Ok(result);
        }
    }
} 