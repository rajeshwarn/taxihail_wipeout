#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IAccountService
    {
        Task<Account> GetMyAccount();

		/// <summary>
		/// Get phone number without authorization
		/// </summary>
		/// <returns></returns>
		Task<CurrentAccountPhoneResponse> GetAccountPhoneNumber(CurrentAccountPhoneRequest currentAccountPhoneRequest);

        Task RegisterAccount(RegisterAccount account);

        Task GetConfirmationCode(ConfirmationCodeRequest request);

        Task ConfirmAccount(ConfirmAccountRequest request);
        Task UpdateBookingSettings(BookingSettingsRequest settings);
        Task<IEnumerable<Address>> GetFavoriteAddresses();
        Task<IList<Address>> GetHistoryAddresses(Guid accountId);
        Task AddFavoriteAddress(SaveAddress address);
        Task UpdateFavoriteAddress(SaveAddress address);
        Task RemoveFavoriteAddress(Guid addressId);
        Task ResetPassword(string emailAddress);
        Task<string> UpdatePassword(UpdatePassword updatePassword);
        Task RemoveAddress(Guid addressId);

        Task<IEnumerable<CreditCardDetails>> GetCreditCards();
        Task AddCreditCard(CreditCardRequest creditCardRequest);
        Task<CreditCardDetails> RemoveCreditCard(Guid creditCardId);
        Task UpdateCreditCard(CreditCardRequest creditCardRequest);
        Task UpdateDefaultCreditCard(DefaultCreditCardRequest defaultCreditCardRequest);
        Task UpdateCreditCardLabel(UpdateCreditCardLabelRequest updateCreditCardLabelRequest);

        Task<NotificationSettings> GetNotificationSettings(Guid accountId);
        Task UpdateNotificationSettings(NotificationSettingsRequest notificationSettingsRequest);

        Task<UserTaxiHailNetworkSettings> GetUserTaxiHailNetworkSettings(Guid accountId);
        Task UpdateUserTaxiHailNetworkSettings(UserTaxiHailNetworkSettingsRequest userTaxiHailNetworkSettingsRequest);
    }
}