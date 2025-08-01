using Azure;
using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class BankService : BaseService, IBankService
    {
        private readonly DataContext _context;
        public BankService(DataContext context)
        {
            _context = context;
        }

        public async Task<APIResponse> CreateBankAsync(CreateBankRequest request)
        {
            var bank = new Bank { Name = request.Name, ShortCode = request.ShortCode };
            _context.Banks.Add(bank);
            await _context.SaveChangesAsync();
            var res = new BankDto { Id = bank.Id, Name = bank.Name , ShortCode = bank.ShortCode };
            return Response.WithStatus("success").WithMessage("Successfully.").WithType(res).GenerateResponse();
        }

        public async Task<APIResponse> UpdateBankAsync(UpdateBankRequest request)
        {
            var bank = await _context.Banks.FindAsync(request.Id);
            if (bank == null) return null;
            bank.Name = request.Name;
            bank.ShortCode = request.ShortCode;
            await _context.SaveChangesAsync();
            var res = new BankDto { Id = bank.Id, Name = bank.Name, ShortCode = bank.ShortCode };
            return Response.WithStatus("success").WithMessage("Successfully.").WithType(res).GenerateResponse();
        }

        public async Task<APIResponse> DeleteBankAsync(int id)
        {
            var bank = await _context.Banks.FindAsync(id);
            //if (bank == null) return false;
            _context.Banks.Remove(bank);
            var res = await _context.SaveChangesAsync();
            return Response.WithStatus("success").WithMessage("Successfully.").WithType(res).GenerateResponse();
        }

        public async Task<APIResponse> GetBankByIdAsync(int id)
        {
            var bank = await _context.Banks.FindAsync(id);
            if (bank == null) return null;
            var res = new BankDto { Id = bank.Id, Name = bank.Name , ShortCode = bank.ShortCode };
            return Response.WithStatus("success").WithMessage("Successfully.").WithType(res).GenerateResponse();
        }

        public async Task<APIResponse> GetAllBanksAsync()
        {
            var res = await _context.Banks.Select(b => new BankDto { Id = b.Id, Name = b.Name , ShortCode = b.ShortCode }).ToListAsync();
            return Response.WithStatus("success").WithMessage("Successfully.").WithType(res).GenerateResponse();
        }
        public async Task<IList<SelectItem>> GetBankSelect()
        {
            var query = _context.Banks.Where(d => d.Deleted == false).OrderBy(s => s.Name);

            var list = await query.Select(a => new SelectItem
            {
                Text = a.Name,
                Value = a.Id.ToString()
            }).ToListAsync();

            return list;
        }
    }
} 