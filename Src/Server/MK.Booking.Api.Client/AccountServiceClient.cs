using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client
{
    public class AccountServiceClient : BaseServiceClient
    {
        public AccountServiceClient(string url, AuthInfo credential)
            : base(url, credential)
        {
        }


        public Account GetMyAccount( )
        {
            var result = Client.Get<Account>("/account/me");
            return result;
        }
        
        public Account GetTestAccount(int index)
        {
            var result = Client.Get<Account>("/account/test/" + index.ToString());
            return result;
        }

        public Guid RegisterAccount(RegisterAccount account)
        {
            var result = Client.Post<Account>("/account/register",account);
            return result.Id;
        }

        public IList<Address> GetFavoriteAddresses(Guid accountId)
        {
            var req = string.Format("/account/{0}/addresses", accountId.ToString());
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public void AddFavoriteAddress(SaveFavoriteAddress address)
        {
            var req = string.Format("/account/{0}/addresses", address.AccountId);
            var response = Client.Post<string>(req, address);
        }

        public void UpdateFavoriteAddress(SaveFavoriteAddress address)
        {
            var req = string.Format("/account/{0}/addresses/{1}", address.AccountId, address.Id);
            var response = Client.Put<string>(req, address);
        }

        public void RemoveFavoriteAddress(Guid accountId, Guid addressId)
        {
            var req = string.Format("/account/{0}/addresses/{1}", accountId, addressId);
            var response = Client.Delete<string>(req);
        }

        public void ResetPassword(Guid accountId)
        {
            var req = string.Format("/account/{0}/resetpassword", accountId);
            var response = Client.Post<string>(req,null);
        }
    }
}
