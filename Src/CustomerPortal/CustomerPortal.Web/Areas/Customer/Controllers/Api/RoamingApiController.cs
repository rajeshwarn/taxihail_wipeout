using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Extensions;
using MongoRepository;
using Newtonsoft.Json;

namespace CustomerPortal.Web.Areas.Customer.Controllers.Api
{
    public class RoamingApiController : ApiController
    {
        private readonly IRepository<TaxiHailNetworkSettings> _networkRepository;
        private readonly IRepository<Company> _companyRepository;


        public RoamingApiController() 
            : this(new MongoRepository<TaxiHailNetworkSettings>(), new MongoRepository<Company>())
        {
        }

        public RoamingApiController(IRepository<TaxiHailNetworkSettings> networkRepository, IRepository<Company> companyRepository)
        {
            _networkRepository = networkRepository;
            _companyRepository = companyRepository;
        }

        [Route("api/customer/roaming/market")]
        public HttpResponseMessage GetCompanyMarket(string companyId, double latitude, double longitude)
        {
            string companyMarket = null;

            var userPosition = new MapCoordinate
            {
                Latitude = latitude,
                Longitude = longitude
            };

            // Get all companies in network
            var companiesInNetwork = _networkRepository.Where(n => n.IsInNetwork).ToArray();

            // Find the first company that includes the user position
            // (it doesn't matter which one because they will all share the same market key anyway)
            var localCompany = companiesInNetwork.FirstOrDefault(x => x.Region.Contains(userPosition));
            if (localCompany != null)
            {
                // Check if we have changed market
                var homeCompany = _networkRepository.FirstOrDefault(n => n.Id == companyId);
                if (homeCompany != null)
                {
                    if (localCompany.Market != homeCompany.Market)
                    {
                        companyMarket = localCompany.Market;
                    }
                }
            }

            if (companyMarket != null)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(companyMarket)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [Route("api/customer/roaming/marketfleets")]
        public HttpResponseMessage GetMarketFleets(string market)
        {
            var marketFleets = new List<NetworkFleetResponse>();

            // Get all companies in the market
            var companiesInMarket = _networkRepository.Where(n => n.IsInNetwork && n.Market == market);

            foreach (var availableCompany in companiesInMarket)
            {
                var company = _companyRepository.FirstOrDefault(c => c.CompanyKey == availableCompany.Id);
                if (company != null)
                {
                    marketFleets.Add(new NetworkFleetResponse
                    {
                        CompanyKey = company.CompanyKey,
                        CompanyName = company.CompanyName,
                        FleetId = availableCompany.FleetId,
                        IbsPassword = company.IBS.Password,
                        IbsUserName = company.IBS.Username,
                        IbsUrl = company.IBS.ServiceUrl
                    });
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(marketFleets))
            };
        }
    }
}