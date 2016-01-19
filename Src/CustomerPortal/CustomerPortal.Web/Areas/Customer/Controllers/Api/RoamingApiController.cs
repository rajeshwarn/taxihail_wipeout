using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Extensions;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoRepository;
using Newtonsoft.Json;

namespace CustomerPortal.Web.Areas.Customer.Controllers.Api
{
    public class RoamingApiController : ApiController
    {
        private readonly IRepository<TaxiHailNetworkSettings> _networkRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Market> _marketRepository;
         
        public RoamingApiController() 
            : this(new MongoRepository<TaxiHailNetworkSettings>(), new MongoRepository<Company>(), new MongoRepository<Market>())
        {
        }

        public RoamingApiController(IRepository<TaxiHailNetworkSettings> networkRepository, IRepository<Company> companyRepository, IRepository<Market> marketRepository)
        {
            _networkRepository = networkRepository;
            _companyRepository = companyRepository;
            _marketRepository = marketRepository;
        }

        [Route("api/customer/{companyId}/roaming/networkfleets")]
        public HttpResponseMessage GetRoamingFleetsPreferences(string companyId)
        {
            var homeCompanySettings = _networkRepository.FirstOrDefault(n => n.Id == companyId);
            if (homeCompanySettings == null || !homeCompanySettings.IsInNetwork)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            var companiesFromOtherMarkets = _networkRepository
                .Where(n => n.IsInNetwork
                    && n.Id != homeCompanySettings.Id
                    && n.Market != homeCompanySettings.Market)
                    .ToArray();

            var preferences = new Dictionary<string, List<CompanyPreferenceResponse>>();

            foreach (var roamingCompany in companiesFromOtherMarkets)
            {
                if (!IsFleetAllowed(roamingCompany.FleetId, homeCompanySettings.WhiteListedFleetIds, homeCompanySettings.BlackListedFleetIds))
                {
                    // Roaming company is not allowed by the home company
                    continue;
                }

                var companyPreference = homeCompanySettings.Preferences.FirstOrDefault(p => p.CompanyKey == roamingCompany.Id)
                            ?? new CompanyPreference { CompanyKey = roamingCompany.Id };

                var doesRoamingCompanyAllowUsToDispatch = roamingCompany.Preferences.Any(x => x.CompanyKey == companyId && x.CanAccept);

                // Add the roaming company to its corresponding market in the response dictionnary
                if (!preferences.ContainsKey(roamingCompany.Market))
                {
                    preferences.Add(roamingCompany.Market, new List<CompanyPreferenceResponse>());
                }

                preferences[roamingCompany.Market].Add(new CompanyPreferenceResponse
                {
                    CompanyPreference = companyPreference,
                    CanDispatchTo = doesRoamingCompanyAllowUsToDispatch,
                    FleetId = roamingCompany.FleetId
                });
            }

            var markets = new List<string>(preferences.Keys);
            foreach (var market in markets)
            {
                if (preferences.ContainsKey(market))
                {
                    preferences[market] = preferences[market]
                        .OrderBy(p => p.CompanyPreference.Order == null)
                        .ThenBy(p => p.CompanyPreference.Order)
                        .ToList();
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(preferences))
            };
        }

        [Route("api/customer/roaming/market")]
        public HttpResponseMessage GetCompanyMarket(string companyId, double latitude, double longitude)
        {
            var response = GetCompanyMarketSettingsInternal(companyId, latitude, longitude);
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response.Market)) 
            };
        }

        /// <summary>
        /// Like GetCompanyMarket but returns dispatcher settings
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        [Route("api/customer/roaming/marketsettings")]
        public HttpResponseMessage GetCompanyMarketSettings(string companyId, double latitude, double longitude)
        {
            var response = GetCompanyMarketSettingsInternal(companyId, latitude, longitude);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }

        private CompanyMarketSettingsResponse GetCompanyMarketSettingsInternal(string companyId, double latitude, double longitude)
        {
            var response = new CompanyMarketSettingsResponse();

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
                response = GetMarketSettings(localCompany.Market);

                // Check if we have changed market
                var homeCompany = _networkRepository.FirstOrDefault(n => n.Id == companyId);
                if (homeCompany == null                             // company not found in network
                    || !homeCompany.IsInNetwork                     // company is not network enabled
                    || localCompany.Market == homeCompany.Market)   // company is in local market
                {
                    response.Market = null;
                }
            }

            return response;
        }

        /// <summary>
        /// Returns all the fleets from a market
        /// </summary>
        /// <param name="companyId">The home company id</param>
        /// <param name="market">The market to return the fleets from</param>
        /// <returns>The NetworkFleets from the market</returns>
        [Route("api/customer/{companyId}/roaming/marketfleets")]
        public HttpResponseMessage GetMarketFleets(string companyId, string market)
        {
            var marketFleets = new List<NetworkFleetResponse>();

            // Current company
            var currentCompanySettings = _networkRepository.FirstOrDefault(n => n.Id == companyId);
            if (currentCompanySettings == null || !currentCompanySettings.IsInNetwork)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            var dispatchableCompany = currentCompanySettings.Preferences.Where(p => p.CanDispatch).ToArray();

            // Get all companies in the market that accepts dispatch for the company
            var companiesInMarket = _networkRepository.Where(n => n.IsInNetwork
                && n.Market == market
                && n.Preferences.Any(p => p.CompanyKey == companyId && p.CanAccept)).ToList();

            var dispatchableCompaniesInMarket =
                companiesInMarket.Where(c =>
                    dispatchableCompany.Any(p => p.CompanyKey == c.Id)
                    && IsFleetAllowed(c.FleetId, currentCompanySettings.WhiteListedFleetIds, currentCompanySettings.BlackListedFleetIds))
                    .ToArray();

            foreach (var availableCompany in dispatchableCompaniesInMarket)
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
                        IbsUrl = company.IBS.ServiceUrl,
                        IbsTimeDifference = GetIbsTimeDifference(company)
                    });
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(marketFleets))
            };
        }

        /// <summary>
        /// Returns a single fleet/company from an external market
        /// </summary>
        /// <param name="market">The market of the fleet/company</param>
        /// <param name="fleetId">The fleet id of the fleet</param>
        /// <returns>The matching NetworkFleet</returns>
        [Route("api/customer/roaming/marketfleet")]
        public HttpResponseMessage GetMarketFleet(string market, int fleetId)
        {
            // Get all companies in the market
            var companiesInMarket = _networkRepository.Where(n => n.IsInNetwork && n.Market == market);
            var fleet = companiesInMarket.FirstOrDefault(c => c.FleetId == fleetId);

            if (fleet != null)
            {
                var company = _companyRepository.FirstOrDefault(c => c.CompanyKey == fleet.Id);
                if (company != null)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(
                            new NetworkFleetResponse
                            {
                                CompanyKey = company.CompanyKey,
                                CompanyName = company.CompanyName,
                                FleetId = fleet.FleetId,
                                IbsPassword = company.IBS.Password,
                                IbsUserName = company.IBS.Username,
                                IbsUrl = company.IBS.ServiceUrl,
                                IbsTimeDifference = GetIbsTimeDifference(company)
                            }))
                    };
                }                
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(null))
            };
        }

        private long GetIbsTimeDifference(Company company)
        {
            long ibsTimeDifference = 0;

            var ibsTimeDifferenceString = company.CompanySettings.FirstOrDefault(s => s.Key == "IBS.TimeDifference");
            if (ibsTimeDifferenceString != null)
            {
                long.TryParse(ibsTimeDifferenceString.Value, out ibsTimeDifference);
            }

            return ibsTimeDifference;
        }

        private bool IsFleetAllowed(int fleetId, string whiteListedFleetIds, string blackListedFleetIds)
        {
            bool allowed = true;

            if (!string.IsNullOrEmpty(blackListedFleetIds))
            {
                var blackList = Regex.Replace(blackListedFleetIds, @"\s+", string.Empty).Split(',');
                allowed &= !blackList.Contains(fleetId.ToString());
            }

            if (!string.IsNullOrEmpty(whiteListedFleetIds))
            {
                var whiteList = Regex.Replace(whiteListedFleetIds, @"\s+", string.Empty).Split(',');
                allowed &= whiteList.Contains(fleetId.ToString());
            }

            return allowed;
        }

        private CompanyMarketSettingsResponse GetMarketSettings(string marketName)
        {
            if (marketName == null)
            {
                return new CompanyMarketSettingsResponse();
            }

            var marketSettings = _marketRepository.GetMarket(marketName);

            return marketSettings != null
                ? new CompanyMarketSettingsResponse()
                {
                    Market = marketName,
                    DispatcherSettings = marketSettings.DispatcherSettings,
                    EnableDriverBonus = marketSettings.EnableDriverBonus,
                    ReceiptFooter = marketSettings.ReceiptFooter,
                    EnableAppFareEstimates = marketSettings.EnableAppFareEstimates,
                    FlatRate = marketSettings.FlatRate,
                    KilometricRate = marketSettings.KilometricRate,
                    KilometerIncluded = marketSettings.KilometerIncluded,
                    MinimumRate = marketSettings.MinimumRate,
                    MarginOfError = marketSettings.MarginOfError,
                    PerMinuteRate = marketSettings.PerMinuteRate
                }
                : new CompanyMarketSettingsResponse();
        }
    }
}