using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;


namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAccountService
    {        
        void UpdateSettings(BookingSettings settings, Guid? creditCardId, int? tipPercent);
        
		Task<Account> SignIn(string email, string password);
        
		Task<Account> GetFacebookAccount(string facebookId);
        
		Task<Account> GetTwitterAccount(string twitterId);
        
        void ClearCache();

        [Obsolete("Use async method instead")]
        ReferenceData GetReferenceData();

        Task<ReferenceData> GetReferenceDataAsync();

        void ClearReferenceData();
        
        Account RefreshAccount();
        
        Account CurrentAccount { get; }
        
        IEnumerable<ListItem> GetCompaniesList( );
        
        IEnumerable<ListItem> GetVehiclesList(  );
        
        IEnumerable<ListItem> GetPaymentsList(  );
        
        
        void RemoveCreditCard(Guid creditCardId);
        void ResetPassword( string email );
        
        string UpdatePassword( Guid accountId, string currentPassword, string newPassword );
        
        void ResendConfirmationEmail(string email);
        
        bool Register (RegisterAccount data, out string error);

        Address FindInAccountAddresses(double latitude, double longitude);

        IEnumerable<Address> GetHistoryAddresses();
        
        IEnumerable<Address> GetFavoriteAddresses();
        
        void UpdateAddress(Address address);
        
        void DeleteFavoriteAddress(Guid addressId);
        
        void DeleteHistoryAddress(Guid addressId);
        
		Task<IList<Order>> GetHistoryOrders();
		OrderStatusDetail[] GetActiveOrdersStatus();
        
        Order GetHistoryOrder(Guid id);
        
        IEnumerable<CreditCardDetails> GetCreditCards();
        
        void RefreshCache(bool reload);
        
        void SignOut();
        
        bool AddCreditCard(CreditCardInfos creditCard);

        
    }
}

