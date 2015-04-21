﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;

namespace CustomerPortal.Client
{
    public interface ITaxiHailNetworkServiceClient
    {
        Task<List<CompanyPreferenceResponse>> GetNetworkCompanyPreferences(string companyId);

        Task<Dictionary<string, List<CompanyPreferenceResponse>>> GetRoamingCompanyPreferences(string companyId);

        Task SetNetworkCompanyPreferences(string companyId, CompanyPreference[] companyPreferences);

        Task<List<NetworkFleetResponse>> GetNetworkFleetAsync(string companyId, double? latitude = null, double? longitude = null);

        List<NetworkFleetResponse> GetNetworkFleet(string companyId, double? latitude = null, double? longitude = null);

        string GetCompanyMarket(double latitude, double longitude);

        IEnumerable<NetworkFleetResponse> GetMarketFleets(string companyId, string market);

        NetworkFleetResponse GetMarketFleet(string market, int fleetId);

        IEnumerable<NetworkVehicleResponse> GetMarketVehicleTypes(string companyId = null, string market = null);

        NetworkVehicleResponse GetAssociatedMarketVehicleType(string companyId, int networkVehicleId);
    }
}
