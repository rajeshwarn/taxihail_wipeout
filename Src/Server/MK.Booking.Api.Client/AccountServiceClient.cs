using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.Common.Web;

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
            var result = Client.Get<Account>("/accounts/me");
            return result;
        }
        
        public Account GetTestAccount(int index)
        {

            var result = Client.Get<Account>("/accounts/test/" + index.ToString());            
            return result;
        }

        public void RegisterAccount(RegisterAccount account)
        {
            var result = Client.Post<Account>("/accounts/register", account);
                        
        }

        public void UpdateBookingSettings(Guid accountId, BookingSettingsRequest settings)
        {
            Client.Put<string>(string.Format("/accounts/{0}/bookingsettings", accountId), settings);
        }

        public IList<Address> GetFavoriteAddresses(Guid accountId)
        {
            var req = string.Format("/accounts/{0}/addresses", accountId.ToString());
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public IList<Address> GetHistoryAddresses(Guid accountId)
        {
            var req = string.Format("/accounts/{0}/addresses/history", accountId.ToString());
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public void AddFavoriteAddress(SaveFavoriteAddress address)
        {
            var req = string.Format("/accounts/{0}/addresses", address.AccountId);
            var response = Client.Post<string>(req, address);
        }

        public void UpdateFavoriteAddress(SaveFavoriteAddress address)
        {
            var req = string.Format("/accounts/{0}/addresses/{1}", address.AccountId, address.Id);
            var response = Client.Put<string>(req, address);
        }

        public void RemoveFavoriteAddress(Guid accountId, Guid addressId)
        {
            var req = string.Format("/accounts/{0}/addresses/{1}", accountId, addressId);
            var response = Client.Delete<string>(req);
        }

        public void ResetPassword(string emailAddress)
        {
            var req = string.Format("/accounts/resetpassword/{0}", emailAddress);
            var response = Client.Post<string>(req,null);
        }

        
    }
}
