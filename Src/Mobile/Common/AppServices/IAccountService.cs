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

	    void ClearVehicleTypesCache();
        
        Account CurrentAccount { get; }

        Task<IList<VehicleType>> GetVehiclesList(bool refresh = false);

	    void SetMarketVehiclesList(List<VehicleType> marketVehicleTypes);

        Task ResetLocalVehiclesList();

        Task<IList<ListItem>> GetPaymentsList();
        
        Task ResetPassword( string email );
        
		Task<string> UpdatePassword( Guid accountId, string currentPassword, string newPassword );
        
		Task Register (RegisterAccount data);

		Task<Address> FindInAccountAddresses(double latitude, double longitude);

		Task<Address[]> GetHistoryAddresses();
        
		Task<Address[]> GetFavoriteAddresses();
        
		Task UpdateAddress(Address address);
        
        Task DeleteFavoriteAddress(Guid addressId);
        
		Task DeleteHistoryAddress(Guid addressId);
        
		Task<IList<Order>> GetHistoryOrders();

		OrderStatusDetail[] GetActiveOrdersStatus();
        
		Task<Order> GetHistoryOrderAsync(Guid id);
        
        void RefreshCache(bool reload);
        
        void SignOut();
        
		Task<CreditCardDetails> GetCreditCard ();
		Task<bool> AddOrUpdateCreditCard (CreditCardInfos creditCard, bool isUpdate = false);
		Task RemoveCreditCard (bool replacedByPayPal = false);

		Task LinkPayPalAccount(string authCode);
		Task UnlinkPayPalAccount (bool replacedByCreditCard = false);

        Task<NotificationSettings> GetNotificationSettings(bool companyDefaultOnly = false, bool cleanCache = false);
	    Task UpdateNotificationSettings(NotificationSettings notificationSettings);

	    Task<UserTaxiHailNetworkSettings> GetUserTaxiHailNetworkSettings(bool cleanCache = false);

	    Task UpdateUserTaxiHailNetworkSettings(UserTaxiHailNetworkSettings userTaxiHailNetworkSettings);
    }
}

