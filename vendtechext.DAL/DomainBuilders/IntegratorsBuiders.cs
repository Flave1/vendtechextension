using vendtechext.DAL.Models;

namespace vendtechext.DAL.DomainBuilders
{
    public class IntegratorsBuilder
    {
        private Integrator _integrator;

        public IntegratorsBuilder()
        {
            _integrator = new Integrator();
        }

        public IntegratorsBuilder(Integrator integrator)
        {
            _integrator = integrator;
        }

        public IntegratorsBuilder WithId(Guid id)
        {
            _integrator.Id = id;
            return this;
        }

        public IntegratorsBuilder WithFirstName(string firstName)
        {
            _integrator.FirstName = firstName;
            return this;
        }

        public IntegratorsBuilder WithLastName(string lastName)
        {
            _integrator.LastName = lastName;
            return this;
        }

        public IntegratorsBuilder WithPassword(string password)
        {
            _integrator.Password = password;
            return this;
        }

        public IntegratorsBuilder WithPhone(string phone)
        {
            _integrator.Phone = phone;
            return this;
        }

        public IntegratorsBuilder WithBusinessName(string businessName)
        {
            _integrator.BusinessName = businessName;
            return this;
        }
        public IntegratorsBuilder WithEmail(string email)
        {
            _integrator.Email = email;
            return this;
        }

        public IntegratorsBuilder WithClientKey(string clientKey)
        {
            _integrator.Clientkey = clientKey;
            return this;
        }

        public IntegratorsBuilder WithApiKey(string apiKey)
        {
            _integrator.ApiKey = apiKey;
            return this;
        }

        public Integrator Build()
        {
            return _integrator;
        }
    }

}
