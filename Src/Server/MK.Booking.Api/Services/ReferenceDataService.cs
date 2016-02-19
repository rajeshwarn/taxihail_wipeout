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
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration.Impl;
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
        private readonly IAirlineDao _airlineDao;
        private readonly IPickupPointDao _pickupPointDao;

        public ReferenceDataService(
            IIBSServiceProvider ibsServiceProvider,
            ICacheClient cacheClient,
            IServerSettings serverSettings,
            IAirlineDao airlineDao,
            IPickupPointDao pickupPointDao)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _cacheClient = cacheClient;
            _serverSettings = serverSettings;
            _airlineDao = airlineDao;
            _pickupPointDao = pickupPointDao;
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
            var isPaymentOutOfAppDisabled = paymentSettings.IsPaymentOutOfAppDisabled;

            IEnumerable<ListItem> filteredPaymentList = result.PaymentsList;

            if (!isChargeAccountPaymentEnabled)
            {
                filteredPaymentList = filteredPaymentList.Where(x => x.Id != ChargeTypes.Account.Id);
            }
            if (!isPayPalEnabled)
            {
                filteredPaymentList = filteredPaymentList.Where(x => x.Id != ChargeTypes.PayPal.Id);
            }
            if (isPaymentOutOfAppDisabled != OutOfAppPaymentDisabled.None)
            {
                filteredPaymentList = filteredPaymentList.Where(x => x.Id != ChargeTypes.PaymentInCar.Id);
            }

            result.PaymentsList = filteredPaymentList.ToList();

            return result;
        }

        public object Get(ReferenceListRequest request)
        {
            if (request.ListName.Equals("airline", StringComparison.InvariantCultureIgnoreCase))
            {
	            return request.SearchText.HasValue()
		            ? _airlineDao.FindByName(request.SearchText)
		            : _airlineDao.GetAll();
            }

	        if (request.ListName.Equals("pickuppoint", StringComparison.InvariantCultureIgnoreCase))
	        {
		        return request.SearchText.HasValue()
			        ? _pickupPointDao.FindByName(request.SearchText)
			        : _pickupPointDao.GetAll();
	        }

	        throw new InvalidOperationException("Unknown list " + request.ListName);
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