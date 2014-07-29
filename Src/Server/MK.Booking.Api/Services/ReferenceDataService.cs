#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ReferenceDataService : Service
    {
        public const string CacheKey = "IBS.StaticData";
        private readonly ICacheClient _cacheClient;
        private readonly IConfigurationManager _configManager;
        private readonly IStaticDataWebServiceClient _staticDataWebServiceClient;

        public ReferenceDataService(
            IStaticDataWebServiceClient staticDataWebServiceClient,
            ICacheClient cacheClient,
            IConfigurationManager configManager)
        {
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _cacheClient = cacheClient;
            _configManager = configManager;
        }

        public object Get(ReferenceDataRequest request)
        {
            var result = _cacheClient.Get<ReferenceData>(CacheKey);

            if (result == null)
            {
                result = GetReferenceData();
                _cacheClient.Add(CacheKey, result);
            }

            if (!request.WithoutFiltering)
            {
                result.VehiclesList = FilterReferenceData(result.VehiclesList, "IBS.ExcludedVehicleTypeId");
                result.CompaniesList = FilterReferenceData(result.CompaniesList, "IBS.ExcludedProviderId");
            }

            return result;
        }

        private ReferenceData GetReferenceData()
        {
            var companies = _staticDataWebServiceClient.GetCompaniesList();
            IList<ListItem> payments = new List<ListItem>();
            IList<ListItem> vehicles = new List<ListItem>();


            foreach (var company in companies)
            {
                payments.AddRange(ChargeTypesClient.GetPaymentsList(company));
                vehicles.AddRange(_staticDataWebServiceClient.GetVehiclesList(company));
            }

            var equalityComparer = new ListItemEqualityComparer();
            var result = new ReferenceData
            {
                CompaniesList = companies.Distinct(equalityComparer).ToArray(),
                PaymentsList = payments.Distinct(equalityComparer).ToArray(),
                VehiclesList = vehicles.Distinct(equalityComparer).ToArray(),
            };

            return result;
        }

        private IList<ListItem> FilterReferenceData(IEnumerable<ListItem> reference, string settingName)
        {
            var excludedVehicleTypeId = _configManager.GetSetting(settingName);
            var excluded = excludedVehicleTypeId.IsNullOrEmpty()
                ? new int[0]
                : excludedVehicleTypeId.Split(';').Select(int.Parse).ToArray();

            return reference.Where(c => excluded.None(e => e == c.Id)).ToList();
        }

        private class ListItemEqualityComparer : EqualityComparer<ListItem>
        {
            public override bool Equals(ListItem x, ListItem y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Id == y.Id;
            }

            public override int GetHashCode(ListItem obj)
            {
                if (obj == null) return 0;
                return obj.Id.GetHashCode();
            }
        }
    }
}