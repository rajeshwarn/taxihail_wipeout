#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http.Extensions;
using AccountCharge = apcurium.MK.Booking.Api.Contract.Resources.AccountCharge;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AdministrationServiceClient : BaseServiceClient
    {
        public AdministrationServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService, null)
        {
        }

        public Task GrantAdminAccess(GrantAdminRightRequest request)
        {
            var req = "/account/grantadmin";
            return Client.Put(req, request);
        }

        public Task GrantSupportAccess(GrantSupportRightRequest request)
        {
            var req = "/account/grantsupport";
            return Client.Put(req, request);
        }

        public Task<IList<Address>> GetDefaultFavoriteAddresses()
        {
            var req = "/admin/addresses";
            return Client.Get(req).Deserialize<IList<Address>>();
        }

        public Task AddDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = "/admin/addresses";
            return Client.Post(req, address);
        }

        public Task UpdateDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = string.Format("/admin/addresses/{0}", address.Id);
            return Client.Put(req, address);
        }

        public Task RemoveDefaultFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/admin/addresses/{0}", addressId);
            return Client.Delete(req);
        }


        public Task AddPopularAddress(PopularAddress address)
        {
            var req = "/admin/popularaddresses";
            return Client.Post(req, address);
        }

        public Task UpdatePopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses/{0}", address.Id);
            return Client.Put(req, address);
        }

        public Task RemovePopularAddress(Guid addressId)
        {
            var req = string.Format("/admin/popularaddresses/{0}", addressId);
            return Client.Delete(req);
        }

        public Task<IList<Address>> GetPopularAddresses()
        {
            var req = "/popularaddresses";
            return Client.Get(req).Deserialize<IList<Address>>();
        }

        public Task<IEnumerable> GetAllAppSettings()
        {
            var req = "/settings";
            return Client.Get(req).Deserialize<IEnumerable>();
        }

        public Task AddOrUpdateAppSettings(ConfigurationsRequest appReq)
        {
            var req = "/settings";
            return Client.Post(req, appReq);
        }

        public Task<string> CreateAccountCharge(AccountChargeRequest request)
        {
            var req = "/admin/accountscharge";
            return Client.Post(req, request).Deserialize<string>();
        }

        public Task<AccountCharge[]>  GetAccountsCharge()
        {
            var req = "/admin/accountscharge";
            return Client.Get(req).Deserialize<AccountCharge[]>();
        }

        public Task<AccountCharge> GetAccountCharge(string accountNumber)
        {
            var req = string.Format("/admin/accountscharge/" + accountNumber);
            return Client.Get(req).Deserialize<AccountCharge>();
        }

        public Task UpdateAccountCharge(AccountChargeRequest request)
        {
            var req = "/admin/accountscharge";
            return Client.Put(req, request);
        }

        public Task DeleteAccountCharge(string accountNumber)
        {
            var req = string.Format("/admin/accountscharge/" + accountNumber);
            return Client.Delete(req);
        }

        public Task<IbsChargeAccount> GetChargeAccount(string accountNumber, string customerNumber)
        {
            var req = string.Format("/admin/ibschargeaccount/{0}/{1}", accountNumber, customerNumber);
            return Client.Get(req).Deserialize<IbsChargeAccount>();
        }

        public Task<IbsChargeAccountValidation> ValidateChargeAccount(IbsChargeAccountValidationRequest validationRequest)
        {
            var req = "/admin/ibschargeaccount/";
            return Client.Post(req, validationRequest).Deserialize<IbsChargeAccountValidation>();
        }

        public Task<IEnumerable<IbsChargeAccount>> GetAllChargeAccount()
        {
            var req = "/admin/ibschargeaccount/all";
            return Client.Get(req).Deserialize<IEnumerable<IbsChargeAccount>>();
        }
    }
}