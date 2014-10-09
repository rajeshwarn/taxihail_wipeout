#region

using System;
using CustomerPortal.Web.Entities;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Domain
{
    public class CompanyService : ICompanyService
    {
        private readonly string _companyId;
        private readonly MongoRepository<Company> _repository;

        public CompanyService(string companyId)
        {
            if (companyId == null) throw new ArgumentNullException("companyId");
            _companyId = companyId;
            _repository = new MongoRepository<Company>();
        }

        public Company GetCompany()
        {
            return _repository.GetById(_companyId);
        }

        public void UpdateQuestionnaire(Questionnaire model)
        {
            var company = _repository.GetById(_companyId);
            company.Application = model;
            _repository.Update(company);
        }

        public void UpdateStoreSettings(StoreSettings model)
        {
            var company = _repository.GetById(_companyId);
            company.Store = model;
            _repository.Update(company);
        }

        public void UpdateAppleAppStoreCredentials(AppleStoreCredentials model)
        {
            var company = _repository.GetById(_companyId);
            company.AppleAppStoreCredentials = model;
            _repository.Update(company);
        }

        public void UpdateGooglePlayCredentials(AndroidStoreCredentials model)
        {
            var company = _repository.GetById(_companyId);
            company.GooglePlayCredentials = model;
            _repository.Update(company);
        }

        public void UpdateAppDescription(string appDescription)
        {
            var company = _repository.GetById(_companyId);
            company.AppDescription = appDescription;
            _repository.Update(company);
        }

        public void ApproveLayouts()
        {
            var company = _repository.GetById(_companyId);
            company.LayoutsApprovedOn = DateTime.UtcNow;
            company.LayoutRejected.Clear();
            company.Status = AppStatus.LayoutCompleted;
            _repository.Update(company);
        }
    }
}