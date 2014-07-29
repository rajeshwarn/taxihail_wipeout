using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common.Entity;

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
        
        Account RefreshAccount();
        
        Account CurrentAccount { get; }
        
		Task<IList<ListItem>> GetCompaniesList();
        
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
        
		[Obsolete("Migrate to async/await")]
		Order GetHistoryOrder(Guid id);
		Task<Order> GetHistoryOrderAsync(Guid id);
        
        void RefreshCache(bool reload);
        
        void SignOut();
        
		Task<CreditCardDetails> GetCreditCard ();
		Task<bool> AddCreditCard (CreditCardInfos creditCard);
		Task<bool> UpdateCreditCard (CreditCardInfos creditCard);
		Task RemoveCreditCard ();

		void LogApplicationStartUp ();
    }
}

