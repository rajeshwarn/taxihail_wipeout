#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IAccountServiceClient
    {
        Task<Account> GetMyAccount();
        Task RegisterAccount(RegisterAccount account);
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
        Task RemoveCreditCard();
        Task UpdateCreditCard(CreditCardRequest creditCardRequest);

        Task<NotificationSettings> GetNotificationSettings(Guid accountId);
        Task UpdateNotificationSettings(NotificationSettingsRequest notificationSettingsRequest);

		Task LogApplicationStartUp(LogApplicationStartUpRequest request);
    }
}