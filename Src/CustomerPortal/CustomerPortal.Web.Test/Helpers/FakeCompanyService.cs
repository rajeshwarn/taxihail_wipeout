#region

using System;
using CustomerPortal.Web.Domain;
using CustomerPortal.Web.Entities;

#endregion

namespace CustomerPortal.Web.Test.Helpers
{
    public class FakeCompanyService : ICompanyService
    {
        private readonly Company _company;

        public FakeCompanyService(Company company)
        {
            _company = company;
        }

        public Company GetCompany()
        {
            return _company;
        }

        public void UpdateQuestionnaire(Questionnaire model)
        {
            _company.Application = model;
        }

        public void UpdateAppleAppStoreCredentials(AppleStoreCredentials model)
        {
            throw new NotImplementedException();
        }

        public void UpdateGooglePlayCredentials(AndroidStoreCredentials model)
        {
            throw new NotImplementedException();
        }

        public void UpdateStoreSettings(StoreSettings model)
        {
            throw new NotImplementedException();
        }

        public void UpdateAppDescription(string appDescription)
        {
            throw new NotImplementedException();
        }

        public void ApproveLayouts()
        {
            throw new NotImplementedException();
        }
    }
}