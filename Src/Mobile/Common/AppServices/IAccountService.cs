
using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;


namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAccountService
	{
	    bool CheckSession();

        void UpdateSettings(BookingSettings settings, Guid? creditCardId, double? tipAmount, double? tipPercent);

        Account GetAccount(string email, string password);

	    Account GetFacebookAccount(string facebookId);

	    Account GetTwitterAccount(string twitterId);

        void ClearCache();

        ReferenceData GetReferenceData();
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

        #region Addresses
        Address FindInAccountAddresses(double latitude, double longitude);

        IEnumerable<Address> GetHistoryAddresses();

        IEnumerable<Address> GetFavoriteAddresses();

        void UpdateAddress(Address address);

	    void DeleteFavoriteAddress(Guid addressId);

        void DeleteHistoryAddress(Guid addressId);
        #endregion

        #region Orders
        IEnumerable<Order> GetHistoryOrders();
        
        Order GetHistoryOrder(Guid id);
        #endregion

        #region Payment

        IEnumerable<CreditCardDetails> GetCreditCards();

        #endregion

        void RefreshCache(bool reload);

        void SignOut();

        void AddCreditCard(CreditCardInfos creditCard);

        
    }
}

