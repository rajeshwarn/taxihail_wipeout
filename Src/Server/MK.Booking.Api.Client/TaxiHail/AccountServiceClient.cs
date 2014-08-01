#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Api.Client.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AccountServiceClient : BaseServiceClient, IAccountServiceClient
    {
        private readonly IPaymentServiceClient _paymentService;

        public AccountServiceClient(string url, string sessionId, IPackageInfo packageInfo,
            IPaymentServiceClient tokenizationService = null)
            : base(url, sessionId, packageInfo)
        {
            _paymentService = tokenizationService;
        }


        public Task<Account> GetMyAccount()
        {
            var result = Client.GetAsync<Account>("/account");
            return result;
        }

        public Task RegisterAccount(RegisterAccount account)
        {
            return Client.PostAsync<Account>("/account/register", account);
        }

        public Task ConfirmAccount(ConfirmAccountRequest request)
        {
			var uri = string.Format ("/account/confirm/{0}/{1}/{2}", 
									request.EmailAddress, 
									request.ConfirmationToken, 
									request.IsSMSConfirmation);
			return Client.GetAsync<string>(uri);
        }

        public Task UpdateBookingSettings(BookingSettingsRequest settings)
        {
            return Client.PutAsync<string>("/account/bookingsettings", settings);
        }

        public Task<IEnumerable<Address>> GetFavoriteAddresses()
        {
            var req = string.Format("/account/addresses");
            var addresses = Client.GetAsync<IEnumerable<Address>>(req);
            return addresses;
        }

        public Task<IList<Address>> GetHistoryAddresses(Guid accountId)
        {
            var req = string.Format("/account/addresses/history");
            var addresses = Client.GetAsync<IList<Address>>(req);
            return addresses;
        }

        public Task AddFavoriteAddress(SaveAddress address)
        {
            var req = string.Format("/account/addresses");
            return Client.PostAsync<string>(req, address);
        }

        public Task UpdateFavoriteAddress(SaveAddress address)
        {
            var req = string.Format("/account/addresses/{0}", address.Id);
            return Client.PutAsync<string>(req, address);
        }

        public Task RemoveFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/account/addresses/{0}", addressId);
            return Client.DeleteAsync<string>(req);
        }

        public Task ResetPassword(string emailAddress)
        {
            var req = string.Format("/account/resetpassword/{0}", emailAddress);
            return Client.PostAsync<string>(req, new object());
        }

        public Task<string> UpdatePassword(UpdatePassword updatePassword)
        {
            var req = string.Format("/accounts/{0}/updatepassword", updatePassword.AccountId);
            var response = Client.PostAsync<string>(req, updatePassword);
            return response;
        }

        public Task RemoveAddress(Guid addressId)
        {
            var req = string.Format("/account/addresses/history/{0}", addressId);
            return Client.DeleteAsync<string>(req);
        }

        public Task<IEnumerable<CreditCardDetails>> GetCreditCards()
        {
            return Client.GetAsync<IEnumerable<CreditCardDetails>>("/account/creditcards");
        }

        public Task AddCreditCard(CreditCardRequest creditCardRequest)
        {
            return Client.PostAsync<string>("/account/creditcards", creditCardRequest);
        }

        public async Task UpdateCreditCard(CreditCardRequest creditCardRequest)
        {
            // unregister previous card(s) except the current token in case the token did not change
            await UnregisterTokenizedCards (creditCardRequest.Token);

            await Client.PutAsync<string> ("/account/creditcards", creditCardRequest);
        }

        public async Task RemoveCreditCard()
        {
            await UnregisterTokenizedCards ();

            // server-side, this should delete every card of the user
            await Client.DeleteAsync<string>("/account/creditcards");
        }

        private async Task UnregisterTokenizedCards(string skipThisToken)
        {
            // previously, it was possible to add multiple cards, this is why we unregister every card here
            var cards = await GetCreditCards ();
            foreach (var card in cards)
            {
                if (!string.IsNullOrWhiteSpace(card.Token) && card.Token != skipThisToken)
                {
                    await _paymentService.ForgetTokenizedCard(card.Token);
                }
            }
        }

        public Task<Account> GetTestAccount(int index)
        {
            var result = Client.GetAsync<Account>("/account/test/" + index);
            return result;
        }

        public Task<Account> CreateTestAccount()
        {
            var result = Client.GetAsync<Account>("/account/test/" + Guid.NewGuid());
            return result;
        }

        public Task<Account> CreateTestAdminAccount()
        {
            var result = Client.GetAsync<Account>("/account/test/admin/" + Guid.NewGuid());
            return result;
        }

		public Task LogApplicationStartUp(LogApplicationStartUpRequest request)
		{
			return Client.PostAsync<string> ("/account/logstartup", request);
		}
    }
}