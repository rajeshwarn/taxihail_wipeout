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
        private readonly IConfigurationManager _configManager;
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
                result = GetReferenceData();
                _cacheClient.Add(CacheKey, result, TimeSpan.FromMinutes(60));
            }

            if (!request.WithoutFiltering)
            {
                result.VehiclesList = FilterReferenceData(result.VehiclesList, "IBS.ExcludedVehicleTypeId");
                result.CompaniesList = FilterReferenceData(result.CompaniesList, "IBS.ExcludedProviderId");
                result.PaymentsList = FilterReferenceData(result.PaymentsList, "IBS.ExcludedPaymentTypeId");
            }

            return result;
        }

        private ReferenceData GetReferenceData()
        {
            var companies = _staticDataWebServiceClient.GetCompaniesList();
            IList<ListItem> payments = new ListItem[0];
            IList<ListItem> vehicles = new ListItem[0];
            IList<ListItem> dropCities = new ListItem[0];
            IList<ListItem> pickCities = new ListItem[0];

            foreach (var company in companies)
            {
                payments = _staticDataWebServiceClient.GetPaymentsList(company);
                vehicles = _staticDataWebServiceClient.GetVehiclesList(company).ToArray();
                dropCities = _staticDataWebServiceClient.GetDropoffCity(company);
                pickCities = _staticDataWebServiceClient.GetPickupCity(company);
            }

            var result = new ReferenceData
                                       {
                                           CompaniesList = companies,
                                           PaymentsList = payments,
                                           VehiclesList = vehicles,
                                           DropoffCityList = dropCities,
                                           PickupCityList = pickCities,
                                       };

            return result;
        }

        private IList<ListItem> FilterReferenceData(IEnumerable<ListItem> reference, string settingName)
        {
            var excludedVehicleTypeId = _configManager.GetSetting(settingName);
            var excluded = excludedVehicleTypeId.IsNullOrEmpty() ? new int[0] : excludedVehicleTypeId.Split(';').Select(int.Parse).ToArray();

            return reference.Where(c => excluded.None(e => e == c.Id)).ToList();
        }
    }
}