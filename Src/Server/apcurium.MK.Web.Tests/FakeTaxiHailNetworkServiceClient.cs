using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;

namespace apcurium.MK.Web.Tests
{
    public class FakeTaxiHailNetworkServiceClient : ITaxiHailNetworkServiceClient
    {
        private readonly string _companyKeyToReturn;

        public FakeTaxiHailNetworkServiceClient(string companyKeyToReturn)
        {
            _companyKeyToReturn = companyKeyToReturn;
        }

        public Task<List<CompanyPreferenceResponse>> GetNetworkCompanyPreferences(string companyId)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, List<CompanyPreferenceResponse>>> GetRoamingCompanyPreferences(string companyId)
        {
            throw new NotImplementedException();
        }

        public Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] companyPreferences)
        {
            throw new NotImplementedException();
        }

        public Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, double? latitude = null, double? longitude = null)
        {
            throw new NotImplementedException();
        }

        public List<NetworkFleetResponse> GetNetworkFleet(string companyId, double? latitude = null, double? longitude = null)
        {
            return new List<NetworkFleetResponse> { new NetworkFleetResponse
            {
                CompanyKey = _companyKeyToReturn, 
                RestApiUrl = "http://cabmatedemo.drivelinq.com:8889/",
                RestApiUser = "EUGENE",
                RestApiSecret = "T!?_asF",
                IbsUrl = "http://mk.drivelinq.com:6928/XDS_IASPI.DLL/soap/",
                IbsUserName = "taxi",
                IbsPassword = "test"
            } };
        }

        public string GetCompanyMarket(double latitude, double longitude)
        {
            return string.Empty;
        }

        public CompanyMarketSettingsResponse GetCompanyMarketSettings(double latitude, double longitude)
        {
            return new CompanyMarketSettingsResponse();
        }

        public IEnumerable<NetworkFleetResponse> GetMarketFleets(string companyId, string market)
        {
            throw new NotImplementedException();
        }

        public NetworkFleetResponse GetMarketFleet(string market, int fleetId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NetworkVehicleResponse> GetMarketVehicleTypes(string companyId = null, string market = null)
        {
            throw new NotImplementedException();
        }

        public NetworkVehicleResponse GetAssociatedMarketVehicleType(string companyId, int networkVehicleId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMarketVehicleType(string companyId, CompanyVehicleType vehicleType)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMarketVehicleMapping(string companyId, Guid id)
        {
            throw new NotImplementedException();
        }

        public CompanyPaymentSettings GetPaymentSettings(string companyId)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePaymentSettings(string companyId, CompanyPaymentSettings companyPaymentSettings)
        {
            throw new NotImplementedException();
        }
    }
}