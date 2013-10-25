using System;
using System.Collections.Generic;
using BraintreeEncryption.Library;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AccountServiceClient : BaseServiceClient, IAccountServiceClient
    {
        private IPaymentServiceClient _paymentService;

        public AccountServiceClient(string url, string sessionId, string userAgent, IPaymentServiceClient tokenizationService =null)
            : base(url, sessionId, userAgent)
        {
            _paymentService = tokenizationService;
        }


        public Account GetMyAccount( )
        {
            var result = Client.Get<Account>("/account");
            return result;
        }
        
        public Account GetTestAccount(int index)
        {
            var result = Client.Get<Account>("/account/test/" + index);
            return result;
        }

        public Account CreateTestAccount()
        {
            var result = Client.Get<Account>("/account/test/" + Guid.NewGuid());
            return result;
        }

        public Account CreateTestAdminAccount()
        {
            var result = Client.Get<Account>("/account/test/admin/" + Guid.NewGuid());
            return result;
        }

        public void RegisterAccount(RegisterAccount account)
        {
            Client.Post<Account>("/account/register", account);         
        }

        public void UpdateBookingSettings(BookingSettingsRequest settings)
        {
            Client.Put<string>(string.Format("/account/bookingsettings"), settings);
        }

        public IList<Address> GetFavoriteAddresses()
        {
            var req = string.Format("/account/addresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public IList<Address> GetHistoryAddresses(Guid accountId)
        {
            var req = string.Format("/account/addresses/history");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public void AddFavoriteAddress(SaveAddress address)
        {
            var req = string.Format("/account/addresses");
            Client.Post<string>(req, address);
        }

        public void UpdateFavoriteAddress(SaveAddress address)
        {
            var req = string.Format("/account/addresses/{0}", address.Id);
            Client.Put<string>(req, address);
        }

        public void RemoveFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/account/addresses/{0}", addressId);
            Client.Delete<string>(req);
        }

        public void ResetPassword(string emailAddress)
        {
            var req = string.Format("/account/resetpassword/{0}", emailAddress);
            Client.Post<string>(req,null);
        }

        public string UpdatePassword(UpdatePassword updatePassword)
        {
            var req = string.Format("/accounts/{0}/updatepassword", updatePassword.AccountId);
            var response = Client.Post<string>(req, updatePassword);
            return response;
        }

        public void RemoveAddress(Guid addressId)
        {
            var req = string.Format("/account/addresses/history/{0}", addressId);
            Client.Delete<string>(req);
        }

        public void AddCreditCard(CreditCardRequest creditCardRequest)
        {
            var req = string.Format("/account/creditcards");
            Client.Post<string>(req, creditCardRequest);
        }

        public IList<CreditCardDetails> GetCreditCards()
        {
            var req = string.Format("/account/creditcards");
            return Client.Get<IList<CreditCardDetails>>(req);
        }

        public void RemoveCreditCard(Guid creditCardId, string cardOnFileToken)
        {
            var req = string.Format("/account/creditcards/" + creditCardId);
            Client.Delete<string>(req);
			if(!string.IsNullOrWhiteSpace(cardOnFileToken))
			{
            	_paymentService.ForgetTokenizedCard(cardOnFileToken);
			}
        }

     

    }
}
