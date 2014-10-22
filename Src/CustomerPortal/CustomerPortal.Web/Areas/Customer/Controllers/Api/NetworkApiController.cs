using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using CustomerPortal.Web.Areas.Customer.Models.RequestResponse;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Extensions;
using MongoRepository;
using Newtonsoft.Json;

namespace CustomerPortal.Web.Areas.Customer.Controllers.Api
{
    public class NetworkApiController : ApiController
    {
        private readonly IRepository<TaxiHailNetworkSettings> _networkRepository;


        public NetworkApiController():this(new MongoRepository<TaxiHailNetworkSettings>())
        {
            
        }

        public NetworkApiController(IRepository<TaxiHailNetworkSettings> repository)
        {
            _networkRepository = repository;
        }

        [Route("api/customer/{companyId}/network")]
        public HttpResponseMessage Get(string companyId)
        {
            var networkSettings = _networkRepository.FirstOrDefault(n => n.Id == companyId);
            
            if (networkSettings == null || !networkSettings.IsInNetwork)
            {
                return null;
            }

            var otherCompaniesInNetwork = _networkRepository.Where(n => n.IsInNetwork && n.Id != networkSettings.Id)
                .ToArray();

            var overlappingCompanies = otherCompaniesInNetwork.Where(n => n.Region.Contains(networkSettings.Region))
                .ToArray();

            var result = new List<CompanyPreferenceResponse>();

            foreach (var nearbyCompany in overlappingCompanies)
            {
                var companyPreference = networkSettings.Preferences.FirstOrDefault(p => p.CompanyKey == nearbyCompany.Id);

                if (companyPreference == null)
                {
                    companyPreference = new CompanyPreference
                    {
                        CompanyKey = nearbyCompany.Id
                    };
                   
                }
                result.Add(new CompanyPreferenceResponse
                {
                    CompanyPreference = companyPreference,
                    CompanyAllowDispatch = false
                });
            }
            
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(result))
            };
            return response;
        }

        [Route("api/customer/{companyId}/network")]
        public HttpResponseMessage Post(string companyId, CompanyPreference[] preferences)
        {
            var networkSetting = _networkRepository.FirstOrDefault(n => n.Id == companyId);
            if (networkSetting == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden); 
            }

            networkSetting.Preferences = preferences.ToList();

            _networkRepository.Update(networkSetting);

            return new HttpResponseMessage(HttpStatusCode.OK);


        }
    }
}