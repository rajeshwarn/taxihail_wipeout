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
        private readonly MongoRepository<TaxiHailNetworkSettings> _networkRepository;

        public NetworkApiController()
        {

            _networkRepository = new MongoRepository<TaxiHailNetworkSettings>();
        }

        [Route("api/customer/{id}/network/")]
        public HttpResponseMessage Get(string id)
        {
            var networkSettings = _networkRepository.FirstOrDefault(n=>n.CompanyId == id);

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