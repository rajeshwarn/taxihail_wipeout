
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

        void UpdateSettings(BookingSettings settings);

        Account GetAccount(string email, string password);

	    Account GetFacebookAccount(string facebookId);

	    Account GetTwitterAccount(string twitterId);
        

	    void UpdateBookingSettings(BookingSettings bookingSettings);
		
		void EnsureListLoaded();

        Account CurrentAccount { get; }

		IEnumerable<ListItem> GetCompaniesList( );
		
		IEnumerable<ListItem> GetVehiclesList(  );
		
		IEnumerable<ListItem> GetPaymentsList(  );

		bool ResetPassword( string email );

		string UpdatePassword( Guid accountId, string currentPassword, string newPassword );

		void ResendConfirmationEmail(string email);
		
		bool Register (RegisterAccount data, out string error);

        Address FindInAccountAddresses(double latitude, double longitude);

        IEnumerable<Address> GetHistoryAddresses();

        IEnumerable<Address> GetFavoriteAddresses();

        void UpdateAddress(Address address);

	    void DeleteFavoriteAddress(Guid addressId);

        void DeleteHistoryAddress(Guid addressId);

        IEnumerable<Order> GetHistoryOrders();

        Order GetHistoryOrder(Guid id);

        void RefreshCache(bool reload);

        void SignOut();
    }
}

