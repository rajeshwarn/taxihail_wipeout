
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
        void UpdateSettings(BookingSettings settings);

        Account GetAccount(string email, string password, out string error);

	    Account GetFacebookAccount(string facebookId, out string error);

	    Account GetTwitterAccount(string twitterId, out string error);

        Account UpdateUser(Account data);

	    void UpdateBookingSettings(BookingSettings bookingSettings);
		
		void EnsureListLoaded();

        Account CurrentAccount { get; }

		IEnumerable<ListItem> GetCompaniesList( );
		
		IEnumerable<ListItem> GetVehiclesList(  );
		
		IEnumerable<ListItem> GetPaymentsList(  );

		bool ResetPassword( string email );

		void ResendConfirmationEmail(string email);
		
		bool Register (RegisterAccount data, out string error);

        Address FindInAccountAddresses(double latitude, double longitude);

        IEnumerable<Address> GetHistoryAddresses();

        IEnumerable<Address> GetFavoriteAddresses();

        void UpdateAddress(Address address);

        void DeleteAddress(Guid addressId);

        IEnumerable<Order> GetHistoryOrders();

        Order GetHistoryOrder(Guid id);

        void RefreshCache();
    }
}

