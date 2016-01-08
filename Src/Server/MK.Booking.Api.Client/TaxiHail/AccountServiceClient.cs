using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;
using System.Linq;
#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AccountServiceClient : BaseServiceClient, IAccountServiceClient
    {
        private readonly IPaymentServiceClient _paymentService;

        public AccountServiceClient(string url, string sessionId, IPackageInfo packageInfo, IPaymentServiceClient tokenizationService = null)
            : base(url, sessionId, packageInfo)
        {
            _paymentService = tokenizationService;
        }

        public Task<Account> GetMyAccount()
        {
            var result = Client.GetAsync<Account>("/account");
            return result;
        }

		/// <summary>
		/// Get phone number without authorization
		/// </summary>
		/// <returns></returns>
		public Task<CurrentAccountPhoneResponse> GetAccountPhoneNumber(CurrentAccountPhoneRequest currentAccountPhoneRequest)
		{
			var uri = string.Format("/account/phone/{0}", currentAccountPhoneRequest.Email);
			return Client.GetAsync<CurrentAccountPhoneResponse>(uri);
		}

        public Task RegisterAccount(RegisterAccount account)
        {
            return Client.PostAsync<Account>("/account/register", account);
        }

        public Task GetConfirmationCode(ConfirmationCodeRequest request)
        {
            var uri = string.Format("/account/getconfirmationcode/{0}/{1}/{2}", request.Email, request.CountryCode, request.PhoneNumber);
            return Client.GetAsync<string>(uri);
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

        public Task UpdateDefaultCreditCard(DefaultCreditCardRequest defaultCreditCardRequest)
        {
            return Client.PostAsync<string>("/account/creditcard/updatedefault", defaultCreditCardRequest);
        }

        public Task UpdateCreditCardLabel(UpdateCreditCardLabelRequest updateCreditCardLabelRequest)
        {
            return Client.PostAsync<string>("/account/creditcard/updatelabel", updateCreditCardLabelRequest);
        }

        public Task AddCreditCard(CreditCardRequest creditCardRequest)
        {
            return Client.PostAsync<string>("/account/creditcards", creditCardRequest);
        }

        public async Task UpdateCreditCard(CreditCardRequest creditCardRequest)
        {
            // unregister previous card(s) except the current token in case the token did not change
            await UnregisterTokenizedCards (creditCardRequest.CreditCardId);

			await Client.PostAsync<string> ("/account/creditcards", creditCardRequest);
        }

        public async Task<NotificationSettings> GetNotificationSettings(Guid accountId)
        {
            var req = string.Format("/settings/notifications/{0}", accountId);
            return await Client.GetAsync<NotificationSettings>(req);
        }

        public async Task UpdateNotificationSettings(NotificationSettingsRequest notificationSettingsRequest)
        {
            string req = string.Format("/settings/notifications/{0}", notificationSettingsRequest.AccountId);
            await Client.PostAsync<string>(req, notificationSettingsRequest);
        }

        public async Task<UserTaxiHailNetworkSettings> GetUserTaxiHailNetworkSettings(Guid accountId)
        {
            var req = string.Format("/settings/taxihailnetwork/{0}", accountId);
            return await Client.GetAsync<UserTaxiHailNetworkSettings>(req);
        }

        public async Task UpdateUserTaxiHailNetworkSettings(UserTaxiHailNetworkSettingsRequest userTaxiHailNetworkSettingsRequest)
        {
            string req = string.Format("/settings/taxihailnetwork/{0}", userTaxiHailNetworkSettingsRequest.AccountId);
            await Client.PostAsync<string>(req, userTaxiHailNetworkSettingsRequest);
        }

        public async Task<CreditCardDetails> RemoveCreditCard(Guid creditCardId, string creditCardToken)
        {
            await UnregisterTokenizedCards (creditCardId, creditCardToken);

            var req = string.Format("/account/creditcards/{0}", creditCardId);
            return await Client.DeleteAsync<CreditCardDetails>(req);
        }

        private async Task UnregisterTokenizedCards(Guid creditCardId, string creditCardToken = null, string skipThisToken = null)
        {
            if (!creditCardToken.HasValue())
            {
                var cards = await GetCreditCards();
                var card = cards.FirstOrDefault(c => c.CreditCardId == creditCardId);
           
                if (card != null)
                {
                    if (card.Token.HasValue() && card.Token != skipThisToken)
                    {
                        await _paymentService.ForgetTokenizedCard(card.Token);
                    }
                }
            }
            else
            {
                if (creditCardToken != skipThisToken)
                {
                    await _paymentService.ForgetTokenizedCard(creditCardToken);
                }
            }

        }

        public Task<Account> GetTestAccount(int index)
        {
            var result = Client.GetAsync<Account>("/account/test/" + index);
            return result;
        }

        public Task<Account> GetAdminTestAccount(int index)
        {
            var result = Client.GetAsync<Account>("/account/test/admin/" + index);
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
    }
}