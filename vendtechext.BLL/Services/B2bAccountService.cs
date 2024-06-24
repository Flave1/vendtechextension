using Microsoft.EntityFrameworkCore.Metadata;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class B2bAccountService : IB2bAccountService
    {
        private readonly DataContext dbcxt;

        public B2bAccountService(DataContext dbcxt)
        {
            this.dbcxt = dbcxt; 
        }

        bool IB2bAccountService.ValidateUser(string clientKey, string apiKey)
        {
            if(string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(clientKey)) 
            { return false; }

            B2bUserAccess credential = dbcxt.B2bUserAccesses.FirstOrDefault(d => d.Clientkey.Contains(clientKey) && d.Apikey.Contains(apiKey));
            if(credential == null) { return false; } return true;
        }
    }
}
