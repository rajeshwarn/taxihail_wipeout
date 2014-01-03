#region

using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IAccountServiceClient
    {
        Account GetMyAccount();
        void RegisterAccount(RegisterAccount account);
        void UpdateBookingSettings(BookingSettingsRequest settings);
        IList<Address> GetFavoriteAddresses();
        IList<Address> GetHistoryAddresses(Guid accountId);
        void AddFavoriteAddress(SaveAddress address);
        void UpdateFavoriteAddress(SaveAddress address);
        void RemoveFavoriteAddress(Guid addressId);
        void ResetPassword(string emailAddress);
        string UpdatePassword(UpdatePassword updatePassword);
        void RemoveAddress(Guid addressId);
        void AddCreditCard(CreditCardRequest creditCardRequest);
        IList<CreditCardDetails> GetCreditCards();
        void RemoveCreditCard(Guid creditCardId, string token);
    }
}