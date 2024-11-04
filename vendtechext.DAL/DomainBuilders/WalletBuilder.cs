
using vendtechext.DAL.Models;

namespace vendtechext.DAL.DomainBuilders
{
    public class WalletBuilder
    {
        private Wallet _wallet;

        public WalletBuilder()
        {
            _wallet = new Wallet();
        }
        public WalletBuilder(Wallet wallet)
        {
            _wallet = wallet;
        }
        public WalletBuilder SetId(Guid id)
        {
            _wallet.Id = id;
            return this;
        }

        public WalletBuilder SetIntegratorId(Guid integratorId)
        {
            _wallet.IntegratorId = integratorId;
            return this;
        }

        public WalletBuilder SetWalletId(string walletId)
        {
            _wallet.WALLET_ID = walletId;
            return this;
        }

 
        public WalletBuilder SetBookBalance(decimal bookBalance)
        {
            _wallet.BookBalance = bookBalance;
            return this;
        }

        public WalletBuilder SetBalance(decimal balance)
        {
            _wallet.Balance = balance;
            return this;
        }

        public WalletBuilder SetDeleted(bool deleted)
        {
            _wallet.Deleted = deleted;
            return this;
        }

        public WalletBuilder SetCreatedAt(DateTime createdAt)
        {
            _wallet.CreatedAt = createdAt;
            return this;
        }

        public WalletBuilder SetUpdatedAt(DateTime updatedAt)
        {
            _wallet.UpdatedAt = updatedAt;
            return this;
        }

        public Wallet Build()
        {
            return _wallet;
        }
    }
}
