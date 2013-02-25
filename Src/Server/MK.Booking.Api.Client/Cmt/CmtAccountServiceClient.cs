using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
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
            var account = Client.Get<Account>("/account/");
            return account;
        }

        public void RegisterAccount(RegisterAccount account)
        {
            var registerCMT = new AccountCmtRequest()
                {
                    Email = account.Email,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    FacebookId = account.FacebookId,
                    Language = account.Language,
                    Password = account.Password,
                    Phone = account.Phone
                };
            Client.Post<CmtResponse>("/registration", registerCMT);         
        }

        public void UpdateBookingSettings(BookingSettingsRequest settings)
        {
            var registerCMT = new AccountCmtRequest
            {
                Email = settings.Email,
                FirstName = settings.FirstName,
                LastName = settings.LastName,
                Phone = settings.Phone,
                AccountStatus = 0
            };
            Client.Put<string>(string.Format("/account"), registerCMT);
        }

        public IList<Address> GetFavoriteAddresses()
        {
            var req = string.Format("/account/addresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses.Where(x => x.Favorite).ToList();
        }

        public IList<Address> GetHistoryAddresses(Guid accountId)
        {
            var req = string.Format("/account/addresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses.Where(x => !x.Favorite).ToList();
        }

        public void AddFavoriteAddress(SaveAddress request)
        {
            request.Address.Favorite = true;
            var req = string.Format("/account/addresses");
            var response = Client.Post<CmtResponse>(req, request);
        }

        public void UpdateFavoriteAddress(SaveAddress request)
        {
            request.Address.Favorite = true;
            var req = string.Format("/account/addresses/{0}", request.Id);
            var response = Client.Put<CmtResponse>(req, request);
        }

        public void RemoveFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/account/addresses/{0}", addressId);
            var response = Client.Delete<CmtResponse>(req);
        }

        public void ResetPassword(string emailAddress)
        {
            var req = string.Format("/account/resetpassword/{0}", emailAddress);
            var response = Client.Post<string>(req,null);
        }

        public string UpdatePassword(UpdatePassword updatePassword)
        {
            var req = string.Format("/account/updatepassword");
            var response = Client.Post<string>(req, updatePassword);
            return response;
        }

        public void RemoveAddress(Guid addressId)
        {
            var req = string.Format("/account/addresses/{0}", addressId);
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
