using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAccountService
    {        
        void UpdateSettings(BookingSettings settings, Guid? creditCardId, int? tipPercent);
        
		void UpdateAccountNumber (string accountNumber);

		Task<Account> SignIn(string email, string password);
        
		Task<Account> GetFacebookAccount(string facebookId);
        
		Task<Account> GetTwitterAccount(string twitterId);
        
        void ClearCache();

        Task<ReferenceData> GetReferenceData();

        void ClearReferenceData();

	    void ClearVehicleTypesCache();
        
        Account RefreshAccount();
        
        Account CurrentAccount { get; }
        
		Task<IList<VehicleType>> GetVehiclesList();
        
		Task<IList<ListItem>> GetPaymentsList();
        
        void ResetPassword( string email );
        
        string UpdatePassword( Guid accountId, string currentPassword, string newPassword );
        
		Task Register (RegisterAccount data);

		Task<Address> FindInAccountAddresses(double latitude, double longitude);

		Task<Address[]> GetHistoryAddresses();
        
		Task<Address[]> GetFavoriteAddresses();
        
        void UpdateAddress(Address address);
        
        void DeleteFavoriteAddress(Guid addressId);
        
        void DeleteHistoryAddress(Guid addressId);
        
		Task<IList<Order>> GetHistoryOrders();
		OrderStatusDetail[] GetActiveOrdersStatus();
        
		Task<Order> GetHistoryOrderAsync(Guid id);
        
        void RefreshCache(bool reload);
        
        void SignOut();
        
		Task<CreditCardDetails> GetCreditCard ();
		Task<bool> AddCreditCard (CreditCardInfos creditCard);
		Task<bool> UpdateCreditCard (CreditCardInfos creditCard);
		Task RemoveCreditCard ();

        Task<NotificationSettings> GetNotificationSettings(bool companyDefaultOnly = false, bool cleanCache = false);
	    Task UpdateNotificationSettings(NotificationSettings notificationSettings);

		void LogApplicationStartUp ();
    }
}

