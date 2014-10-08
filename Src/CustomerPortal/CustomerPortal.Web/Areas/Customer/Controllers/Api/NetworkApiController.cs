using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
                
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(myNetwork))
            };
            return response;
        }
    }
}