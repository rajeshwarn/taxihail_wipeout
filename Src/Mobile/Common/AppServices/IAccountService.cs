using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;
using apcurium.MK.Common;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAccountService
    {        
        Task UpdateSettings(BookingSettings settings, string email, int? tipPercent);
        
		void UpdateAccountNumber (string accountNumber, string customerNumber);

		Task<Account> SignIn(string email, string password);
        
		Task<Account> GetFacebookAccount(string facebookId);
        
		Task<Account> GetTwitterAccount(string twitterId);
        
        void ClearCache();

        Task<ReferenceData> GetReferenceData();

        void ClearReferenceData();
        
        Account CurrentAccount { get; }

        Task<IList<VehicleType>> GetVehiclesList(bool refresh = false);

        Task ResetLocalVehiclesList();

		IObservable<IList<ListItem>> GetAndObservePaymentsList ();
        
        Task ResetPassword( string email );
        
		Task<string> UpdatePassword( Guid accountId, string currentPassword, string newPassword );
        
		Task Register (RegisterAccount data);

		Task<Address> FindInAccountAddresses(double latitude, double longitude);

		Task<Address[]> GetHistoryAddresses(CancellationToken cancellationToken = default(CancellationToken));
        
		Task<Address[]> GetFavoriteAddresses(CancellationToken cancellationToken = default(CancellationToken));
        
		Task UpdateAddress(Address address);
        
        Task DeleteFavoriteAddress(Guid addressId);
        
		Task DeleteHistoryAddress(Guid addressId);
        
		Task<IList<Order>> GetHistoryOrders();

		Task<OrderStatusDetail[]> GetActiveOrdersStatus();
        
		Task<Order> GetHistoryOrderAsync(Guid id);

		Task<int> GetOrderCountForAppRating();
        
        void RefreshCache(bool reload);
        
        void SignOut();
        
		Task<CreditCardDetails> GetDefaultCreditCard ();
		Task<IEnumerable<CreditCardDetails>> GetCreditCards ();
		Task<bool> AddOrUpdateCreditCard (CreditCardInfos creditCard, string kountSessionId, bool isUpdate = false);
		Task RemoveCreditCard (Guid creditCardId, bool replacedByPayPal = false);
		Task<bool> UpdateDefaultCreditCard(Guid creditCardId);
		Task<bool> UpdateCreditCardLabel(Guid creditCardId, CreditCardLabelConstants label);

		Task LinkPayPalAccount(string authCode);
		Task UnlinkPayPalAccount (bool replacedByCreditCard = false);

        Task<NotificationSettings> GetNotificationSettings(bool companyDefaultOnly = false, bool cleanCache = false);
	    Task UpdateNotificationSettings(NotificationSettings notificationSettings);

	    Task<UserTaxiHailNetworkSettings> GetUserTaxiHailNetworkSettings(bool cleanCache = false);

	    Task UpdateUserTaxiHailNetworkSettings(UserTaxiHailNetworkSettings userTaxiHailNetworkSettings);
    }
}

