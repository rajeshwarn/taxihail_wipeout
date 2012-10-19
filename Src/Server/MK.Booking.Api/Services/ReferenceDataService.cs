using System;
using System.Linq;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class ReferenceDataService : RestServiceBase<ReferenceDataRequest>
    {
        private readonly IStaticDataWebServiceClient _staticDataWebServiceClient;
        private readonly ICacheClient _cacheClient;
        private IConfigurationManager _configManager;
        public const string CacheKey = "IBS.StaticData";

        public ReferenceDataService(IStaticDataWebServiceClient staticDataWebServiceClient, ICacheClient cacheClient, IConfigurationManager configManager)
        {
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _cacheClient = cacheClient;
            _configManager = configManager;
        }

        public override object OnGet(ReferenceDataRequest request)
        {
            var result = _cacheClient.Get<ReferenceData>(CacheKey);

            if (result == null)
            {
                var companies = _staticDataWebServiceClient.GetCompaniesList();
                IList<ListItem> payments = new ListItem[0];
                IList<ListItem> vehicles = new ListItem[0];
                IList<ListItem> dropCities= new ListItem[0];
                IList<ListItem> pickCities= new ListItem[0];

                var excludedVehicleTypeId =_configManager.GetSetting("IBS.ExcludedVehicleTypeId");
                int[] excluded = excludedVehicleTypeId.IsNullOrEmpty() ? new int[0] : excludedVehicleTypeId.Split(';').Select(e => int.Parse(e)).ToArray(); 
                foreach (var company in companies)
                {
                    payments = _staticDataWebServiceClient.GetPaymentsList(company);
                    vehicles = _staticDataWebServiceClient.GetVehiclesList(company).Where( c=> excluded.None( e=> e == c.Id ) ).ToArray() ;
                    dropCities = _staticDataWebServiceClient.GetDropoffCity(company);
                    pickCities = _staticDataWebServiceClient.GetPickupCity(company);                    
                }

                result = new ReferenceData
                             {
                                 CompaniesList = companies,
                                 PaymentsList = payments,
                                 VehiclesList = vehicles,
                                 DropoffCityList = dropCities,
                                 PickupCityList = pickCities,
                             };
                _cacheClient.Add(CacheKey, result, TimeSpan.FromMinutes(60));
            }
            return result;
        }
    }
}