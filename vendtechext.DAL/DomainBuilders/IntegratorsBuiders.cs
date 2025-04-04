﻿using System.Numerics;
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


        public IntegratorsBuilder WithAbout(string about)
        {
            _integrator.About = about;
            return this;
        }
        public IntegratorsBuilder WithLogo(string logo)
        {
            _integrator.Logo = logo;
            return this;
        }
        public IntegratorsBuilder WithBusinessName(string businessName)
        {
            _integrator.BusinessName = businessName;
            return this;
        }
        public IntegratorsBuilder WithApiKey(string apiKey)
        {
            _integrator.ApiKey = apiKey;
            return this;
        }

        public IntegratorsBuilder WithAppUserId(string appUserId)
        {
            _integrator.AppUserId = appUserId;
            return this;
        }

        public IntegratorsBuilder WithDisabled(Boolean disabled)
        {
            _integrator.Disabled = disabled;
            return this;
        }

        public Integrator Build()
        {
            return _integrator;
        }

        public object WithAbout(object about)
        {
            throw new NotImplementedException();
        }
    }

}
