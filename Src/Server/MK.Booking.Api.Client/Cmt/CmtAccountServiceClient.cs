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
            account.Name = account.FirstName + " " + account.LastName;
            return account;
        }

        public void RegisterAccount(RegisterAccount account)
        {
            account.FirstName = account.Name.Split (' ')[0];
            account.LastName = account.Name.Split (' ')[1];
            var registerCMT = new
                {
                    email = account.Email,
                    firstName = account.FirstName,
                    lastName = account.LastName,
                    password = account.Password,
                    phone = account.Phone
                };
            var response = Client.Post<CmtResponse>("/registration", registerCMT);         
        }

        public void UpdateBookingSettings(BookingSettingsRequest settings)
        {
            settings.FirstName = settings.Name.Split (' ')[0];
            settings.LastName = settings.Name.Split (' ')[1];
            var registerCMT = new
            {
                email = settings.Email,
                firstName = settings.FirstName,
                lastName = settings.LastName,
                phone = settings.Phone,
                accountStatus = 0
            };
            Client.Put<string>(string.Format("/account"), registerCMT);
        }

        public IList<Address> GetFavoriteAddresses()
        {
            var req = string.Format("/account/addresses");
            var addresses = Client.Get<IList<Address>>(req).ToList();
            addresses = addresses.Where(x => x.Favorite).ToList();
            addresses.ForEach(x => x.Id = x.AddressId);
            return addresses;
        }

        public IList<Address> GetHistoryAddresses(Guid accountId)
        {
            var req = string.Format("/account/addresses");
            var addresses = Client.Get<IList<Address>>(req).ToList();
            addresses = addresses.Where(x => !x.Favorite).ToList();
            addresses.ForEach(x => x.Id = x.AddressId);
            return addresses;
        }

        public void AddFavoriteAddress(SaveAddress request)
        {
            request.Address.Favorite = true;
            var req = string.Format("/account/addresses");
            var response = Client.Post<CmtResponse>(req, new
                {
                    request.Address.FriendlyName,
                    request.Address.BuildingName,
                    request.Address.AddressType,
                    request.Address.FullAddress,
                    request.Address.Favorite,
                    request.Address.RingCode,
                    request.Address.Latitude,
                    request.Address.Longitude
                });
        }

        public void UpdateFavoriteAddress(SaveAddress request)
        {
            request.Address.Favorite = true;
            var req = string.Format("/account/addresses/{0}", request.Address.Id);
            var response = Client.Put<CmtResponse>(req, new
            {
                AddressId = request.Address.Id,
                request.Address.FriendlyName,
                request.Address.BuildingName,
                request.Address.AddressType,
                request.Address.FullAddress,
                request.Address.Favorite,
                request.Address.RingCode,
                request.Address.Latitude,
                request.Address.Longitude
            });
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
