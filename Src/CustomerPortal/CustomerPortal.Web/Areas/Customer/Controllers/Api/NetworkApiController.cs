using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Extensions;
using MongoRepository;

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

            var myNetwork = _networkRepository.Where(n=>n.IsInNetwork).Where(n => n.Region.Contains(networkSettings.Region)).ToArray();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}