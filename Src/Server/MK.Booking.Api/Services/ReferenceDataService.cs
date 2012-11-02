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
            if (request.WithoutFiltering)
                return GetReferenceData(request.WithoutFiltering);

            var result = _cacheClient.Get<ReferenceData>(CacheKey);

            if (result == null)
            {
                result = GetReferenceData(request.WithoutFiltering);
                _cacheClient.Add(CacheKey, result);
            }

            return result;
        }

        private ReferenceData GetReferenceData(bool withoutFiltering)
        {
            ReferenceData result;

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

            result = new ReferenceData
                         {
                             CompaniesList = withoutFiltering ? companies : FilterReferenceData(companies, "IBS.ExcludedProviderId"),
                             PaymentsList = withoutFiltering ? payments : FilterReferenceData(payments, "IBS.ExcludedPaymentTypeId"),
                             VehiclesList = withoutFiltering ? vehicles : FilterReferenceData(vehicles, "IBS.ExcludedVehicleTypeId"),
                             DropoffCityList =  dropCities,
                             PickupCityList = pickCities,
                         };

            return result;
        }

        private IList<ListItem> FilterReferenceData(IEnumerable<ListItem> reference, string settingName)
        {
            var excludedVehicleTypeId = _configManager.GetSetting(settingName);
            int[] excluded = excludedVehicleTypeId.IsNullOrEmpty()  ? new int[0] : excludedVehicleTypeId.Split(';').Select(int.Parse).ToArray();

            return reference.Where(c => excluded.None(e => e == c.Id)).ToList();
        }
    }
}