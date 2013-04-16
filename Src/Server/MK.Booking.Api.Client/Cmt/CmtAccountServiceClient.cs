using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.Cmt;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtAccountServiceClient : CmtBaseServiceClient, IAccountServiceClient
    {
        public CmtAccountServiceClient(string url, CmtAuthCredentials credentials)
            : base(url, credentials)
        {

        }


        public Account GetMyAccount()
        {
            var accountId = Credentials.AccountId;
            var account = Client.Get<Account>("/accounts/" + accountId);
            return account;
        }

        public void RegisterAccount(RegisterAccount account)
        {
            Client.Post<CmtResponse>("/registration", account);         
        }

        public void UpdateBookingSettings(BookingSettingsRequest settings)
        {
            settings.AccountId = Credentials.AccountId;
            Client.Put<string>(string.Format("/accounts/{0}", Credentials.AccountId), settings);
        }

        public IList<Address> GetFavoriteAddresses()
        {
            var req = string.Format("/accounts/{0}/addresses", Credentials.AccountId);
            var addresses = Client.Get<IList<Address>>(req);
            return addresses.Where(x => x.Favorite).ToList();
        }

        public IList<Address> GetHistoryAddresses(Guid accountId)
        {
            var req = string.Format("/accounts/{0}/addresses", Credentials.AccountId);
            var addresses = Client.Get<IList<Address>>(req);
            return addresses.Where(x => !x.Favorite).ToList();
        }

        public void AddFavoriteAddress(SaveAddress request)
        {
            request.Address.Favorite = true;
            var req = string.Format("/accounts/{0}/addresses", Credentials.AccountId);
            var response = Client.Post<CmtResponse>(req, request);
        }

        public void UpdateFavoriteAddress(SaveAddress request)
        {
            request.Address.Favorite = true;
            var req = string.Format("/accounts/{0}/addresses/{0}", Credentials.AccountId, request.Id);
            var response = Client.Put<CmtResponse>(req, request);
        }

        public void RemoveFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/accounts/{0}/addresses/{1}", Credentials.AccountId, addressId);
            var response = Client.Delete<CmtResponse>(req);
        }

        public void ResetPassword(string emailAddress)
        {
            var req = string.Format("/accounts/resetpassword/{0}", emailAddress);
            var response = Client.Post<string>(req,null);
        }

        public string UpdatePassword(UpdatePassword updatePassword)
        {
            var req = string.Format("/accounts/{0}/updatepassword", updatePassword.AccountId);
            var response = Client.Post<string>(req, updatePassword);
            return response;
        }

        public void RemoveAddress(Guid addressId)
        {
            var req = string.Format("/accounts/{0}/addresses/{1}", Credentials.AccountId, addressId);
            Client.Delete<CmtResponse>(req);
        }
        
        public void AddCreditCard (CreditCardRequest creditCardRequest)
        {
            throw new NotImplementedException ();
        }       

        public IList<CreditCardDetails> GetCreditCards ()
        {
            throw new NotImplementedException ();
        }

        public void RemoveCreditCard (Guid creditCardId)
        {
            throw new NotImplementedException ();
        }
    }
}
