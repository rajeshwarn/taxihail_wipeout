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
using apcurium.MK.Common.Extensions;
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
            return Client.PutAsync<object>("/account/grantadmin", request);
        }

        public Task GrantSupportAccess(GrantSupportRightRequest request)
        {
            return Client.PutAsync<object>("/account/grantsupport", request);
        }

        public Task<IList<Address>> GetDefaultFavoriteAddresses()
        {
            return Client.GetAsync<IList<Address>>("/admin/addresses");
        }

        public Task AddDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            return Client.PostAsync<object>("/admin/addresses", address);
        }

        public Task UpdateDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = string.Format("/admin/addresses/{0}", address.Id);
            return Client.PutAsync<object>(req, address);
        }

        public Task RemoveDefaultFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/admin/addresses/{0}", addressId);
            return Client.DeleteAsync<object>(req);
        }


        public Task AddPopularAddress(PopularAddress address)
        {
            return Client.PostAsync<object>("/admin/popularaddresses", address);
        }

        public Task UpdatePopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses/{0}", address.Id);
            return Client.PutAsync<object>(req, address);
        }

        public Task RemovePopularAddress(Guid addressId)
        {
            var req = string.Format("/admin/popularaddresses/{0}", addressId);
            return Client.DeleteAsync<object>(req);
        }

        public Task<IList<Address>> GetPopularAddresses()
        {
            return Client.GetAsync<IList<Address>>("/popularaddresses");
        }

        public Task<IEnumerable> GetAllAppSettings()
        {
            return Client.GetAsync<IEnumerable>("/settings");
        }

        public Task AddOrUpdateAppSettings(ConfigurationsRequest appReq)
        {
            return Client.PostAsync<object>("/settings", appReq);
        }

        public Task CreateAccountCharge(AccountChargeRequest request)
        {
            return Client.PostAsync<object>("/admin/accountscharge", request);
        }

        public Task<AccountCharge[]>  GetAccountsCharge()
        {
            return Client.GetAsync<AccountCharge[]>("/admin/accountscharge");
        }

        public Task<AccountCharge> GetAccountCharge(string accountNumber)
        {
            var req = string.Format("/admin/accountscharge/" + accountNumber);
            return Client.GetAsync<AccountCharge>(req);
        }

        public Task UpdateAccountCharge(AccountChargeRequest request)
        {
            return Client.PutAsync<object>("/admin/accountscharge", request);
        }

        public Task DeleteAccountCharge(string accountNumber)
        {
            var req = string.Format("/admin/accountscharge/" + accountNumber);
            return Client.DeleteAsync<object>(req);
        }

        public Task<IbsChargeAccount> GetChargeAccount(string accountNumber, string customerNumber)
        {
            var req = string.Format("/admin/ibschargeaccount/{0}/{1}", accountNumber, customerNumber);
            return Client.GetAsync<IbsChargeAccount>(req);
        }

        public Task<IbsChargeAccountValidation> ValidateChargeAccount(IbsChargeAccountValidationRequest validationRequest)
        {
            return Client.PostAsync<IbsChargeAccountValidation>("/admin/ibschargeaccount/", validationRequest);
        }

        public Task<IEnumerable<IbsChargeAccount>> GetAllChargeAccount()
        {
            return Client.GetAsync<IEnumerable<IbsChargeAccount>>("/admin/ibschargeaccount/all");
        }
    }
}