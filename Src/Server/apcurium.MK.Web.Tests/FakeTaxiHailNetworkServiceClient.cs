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

        public Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] companyPreferences)
        {
            throw new NotImplementedException();
        }

        public Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, MapCoordinate coordinate = null)
        {
            throw new NotImplementedException();
        }

        public List<NetworkFleetResponse> GetNetworkFleet(string companyId, MapCoordinate coordinate = null)
        {
            return new List<NetworkFleetResponse> { new NetworkFleetResponse
            {
                CompanyKey = _companyKeyToReturn, 
                IbsUrl = "http://mk.drivelinq.com:6928/XDS_IASPI.DLL/soap/",
                IbsUserName = "taxi",
                IbsPassword = "test"
            } };
        }
    }
}