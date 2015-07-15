#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ReferenceDataService : Service
    {
        public const string CacheKey = "IBS.StaticData";
        private readonly ICacheClient _cacheClient;
        private readonly IServerSettings _serverSettings;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private HttpClient restClient;

        public ReferenceDataService(
            IIBSServiceProvider ibsServiceProvider,
            ICacheClient cacheClient,
            IServerSettings serverSettings)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _cacheClient = cacheClient;
            _serverSettings = serverSettings;
            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            restClient = new HttpClient();
            restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            restClient.Timeout = TimeSpan.FromMilliseconds(10000);
        }

        public object Get(ReferenceDataRequest request)
        {
            var cacheKey = string.Format("{0}{1}", CacheKey, request.CompanyKey);
            var result = _cacheClient.Get<ReferenceData>(cacheKey);

            if (result == null)
            {
                result = GetReferenceData(request.CompanyKey);
                _cacheClient.Add(cacheKey, result);
            }

            if (!request.WithoutFiltering)
            {
                result.VehiclesList = FilterReferenceData(result.VehiclesList, _serverSettings.ServerData.IBS.ExcludedVehicleTypeId);
                result.CompaniesList = FilterReferenceData(result.CompaniesList, _serverSettings.ServerData.IBS.ExcludedProviderId);
            }

            var paymentSettings = _serverSettings.GetPaymentSettings(request.CompanyKey);

            var isChargeAccountPaymentEnabled = paymentSettings.IsChargeAccountPaymentEnabled;
            var isPayPalEnabled = paymentSettings.PayPalClientSettings.IsEnabled;
            var isOutOfAppPaymentDisabled = paymentSettings.IsOutOfAppPaymentDisabled;

            IEnumerable<ListItem> filteredPaymentList = result.PaymentsList;

            if (!isChargeAccountPaymentEnabled)
            {
                filteredPaymentList = filteredPaymentList.Where(x => x.Id != ChargeTypes.Account.Id);
            }
            if (!isPayPalEnabled)
            {
                filteredPaymentList = filteredPaymentList.Where(x => x.Id != ChargeTypes.PayPal.Id);
            }
            if (isOutOfAppPaymentDisabled)
            {
                filteredPaymentList = filteredPaymentList.Where(x => x.Id != ChargeTypes.PaymentInCar.Id);
            }

            result.PaymentsList = filteredPaymentList.ToList();

            return result;
        }

        public object Get(ReferenceListRequest request)
        {
            var getUri = new Uri(_serverSettings.ServerData.Gds.ServiceUrl + "/reference/" + request.ListName.ToLower() +
                (request.SearchText.IsNullOrEmpty() ? "" : "/search/" + request.SearchText));
            var response = restClient.GetAsync(getUri).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
            var resultInfo = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            JArray result;
            if (request.SearchText.IsNullOrEmpty())
            {
                result = JArray.Parse(resultInfo);
            }
            else
            {
                var searchResult = JObject.Parse(resultInfo);
                result = (JArray)searchResult["items"];
            }
            result.ForEach(i => {
                if (i["name"] == null) {
                    i["name"] = i["id"];
                }
            });
            if (request.size > 0)
            {
                result = new JArray(result.Take(request.size));
            }
            if (request.coreFieldsOnly)
            {
                return result.Select(i => new 
                { 
                    id = i["id"].ToString(),
                    type = i["type"].ToString(),
                    name = i["name"].ToString()
                }).ToList();
            }
            else
            {
                return result.ToString(Formatting.None);
            }
        }

        private ReferenceData GetReferenceData(string companyKey)
        {
            var companies = _ibsServiceProvider.StaticData(companyKey).GetCompaniesList();
            var payments = new List<ListItem>();
            var vehicles = new List<ListItem>();

            foreach (var company in companies)
            {
                payments.AddRange(ChargeTypesClient.GetPaymentsList(company));
                vehicles.AddRange(_ibsServiceProvider.StaticData(companyKey).GetVehiclesList(company));
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

        private IList<ListItem> FilterReferenceData(IEnumerable<ListItem> reference, string excludedTypeId)
        {
            var excluded = excludedTypeId.IsNullOrEmpty()
                ? new int[0]
                : excludedTypeId.Split(';').Select(int.Parse).ToArray();

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