using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;

namespace CustomerPortal.Web.Areas.Customer.Controllers.Api
{
    public class PaymentSettingsApiController : ApiController
    {
        private readonly IRepository<PaymentSettings> _paymentSettingsRepository;
        private readonly IRepository<Company> _companyRepository;

        public PaymentSettingsApiController()
            : this(new MongoRepository<PaymentSettings>(),
                   new MongoRepository<Company>())
        {
        }

        public PaymentSettingsApiController(
            IRepository<PaymentSettings> paymentSettingsRepository,
            IRepository<Company> companyRepository)
        {
            _paymentSettingsRepository = paymentSettingsRepository;
            _companyRepository = companyRepository;
        }

        [Route("api/customer/{companyId}/paymentSettings")]
        public HttpResponseMessage GetpaymentSettings(string companyId)
        {
            var company = _companyRepository.GetById(companyId);
            //company.PaymentSettings.

            throw new NotImplementedException();
        }

        [Route("api/customer/{companyId}/paymentSettings")]
        public HttpResponseMessage UpsertPaymentSettings(string companyId, CompanyPaymentSettings companyPaymentSettings)
        {
            var company = _companyRepository.GetById(companyId);
            throw new NotImplementedException();
        }
    }
}