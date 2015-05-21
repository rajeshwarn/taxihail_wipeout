using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Entities;
using MongoRepository;
using Newtonsoft.Json;

namespace CustomerPortal.Web.Areas.Customer.Controllers.Api
{
    public class PaymentSettingsApiController : ApiController
    {
        private readonly IRepository<Company> _companyRepository;

        public PaymentSettingsApiController()
            : this(new MongoRepository<Company>())
        {
        }

        public PaymentSettingsApiController(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        [Route("api/customer/{companyId}/paymentSettings")]
        public HttpResponseMessage GetpaymentSettings(string companyId)
        {
            var company = _companyRepository.GetById(companyId);
            if (company == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("CompanyId {0} doesn't exist.", companyId));
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(company.PaymentSettings))
            };
        }

        [Route("api/customer/{companyId}/paymentSettings")]
        public HttpResponseMessage UpsertPaymentSettings(string companyId, CompanyPaymentSettings companyPaymentSettings)
        {
            var company = _companyRepository.GetById(companyId);
            if (company == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("CompanyId {0} doesn't exist.", companyId));
            }

            company.PaymentSettings.PaymentMode = companyPaymentSettings.PaymentMode;
            company.PaymentSettings.BraintreePaymentSettings = companyPaymentSettings.BraintreePaymentSettings;
            company.PaymentSettings.MonerisPaymentSettings = companyPaymentSettings.MonerisPaymentSettings;
            company.PaymentSettings.CmtPaymentSettings = companyPaymentSettings.CmtPaymentSettings;

            try
            {
                // Save changes
                _companyRepository.Update(company);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpException((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}