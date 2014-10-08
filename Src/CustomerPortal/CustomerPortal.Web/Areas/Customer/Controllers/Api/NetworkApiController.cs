using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CustomerPortal.Web.Areas.Admin.Models.RequestResponse;
using CustomerPortal.Web.Entities;
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

        [Route("api/customer/{companyId}/network/")]
        public HttpResponseMessage Get(string companyId)
        {
            var networkSettings = _networkRepository.FirstOrDefault(n => n.CompanyId == companyId);
            
            if (networkSettings == null || !networkSettings.IsInNetwork)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);    
            }

            var network = _networkRepository.Where(n => n.IsInNetwork && n.Id != networkSettings.Id)
                .ToArray();

            var myNetwork = network.Where(n => n.Region.Contains(networkSettings.Region))
                .ToArray();

            foreach (var nearbyCompany in myNetwork)
            {
                if (!networkSettings.Preferences.Any(p => p.CompanyId == nearbyCompany.CompanyId))
                {
                    networkSettings.Preferences.Add(new CompanyPreference()
                    {
                        CompanyId = nearbyCompany.CompanyId
                    });
                }
            }
            
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(networkSettings.Preferences))
            };
            return response;
        }

        public HttpResponseMessage Post(PostCompanyPreferencesRequest postCompanyPreferences)
        {
            var networkSetting = _networkRepository.FirstOrDefault(n => n.CompanyId == postCompanyPreferences.CompanyId);
            if (networkSetting == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden); 
            }

            networkSetting.Preferences = postCompanyPreferences.Preferences.ToList();

            _networkRepository.Update(networkSetting);

            return new HttpResponseMessage(HttpStatusCode.OK);


        }
    }
}