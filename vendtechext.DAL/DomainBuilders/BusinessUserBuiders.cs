using vendtechext.DAL.Models;

namespace vendtechext.DAL.DomainBuilders
{
    public class BusinessUsersBuilder
    {
        private BusinessUsers _businessUser;

        public BusinessUsersBuilder()
        {
            _businessUser = new BusinessUsers();
        }

        public BusinessUsersBuilder(BusinessUsers businessUser)
        {
            _businessUser = businessUser;
        }

        public BusinessUsersBuilder WithId(Guid id)
        {
            _businessUser.Id = id;
            return this;
        }

        public BusinessUsersBuilder WithFirstName(string firstName)
        {
            _businessUser.FirstName = firstName;
            return this;
        }

        public BusinessUsersBuilder WithLastName(string lastName)
        {
            _businessUser.LastName = lastName;
            return this;
        }

        public BusinessUsersBuilder WithPassword(string password)
        {
            _businessUser.Password = password;
            return this;
        }

        public BusinessUsersBuilder WithPhone(string phone)
        {
            _businessUser.Phone = phone;
            return this;
        }

        public BusinessUsersBuilder WithBusinessName(string businessName)
        {
            _businessUser.BusinessName = businessName;
            return this;
        }
        public BusinessUsersBuilder WithEmail(string email)
        {
            _businessUser.Email = email;
            return this;
        }

        public BusinessUsersBuilder WithClientKey(string clientKey)
        {
            _businessUser.Clientkey = clientKey;
            return this;
        }

        public BusinessUsersBuilder WithApiKey(string apiKey)
        {
            _businessUser.ApiKey = apiKey;
            return this;
        }

        public BusinessUsers Build()
        {
            return _businessUser;
        }
    }

}
