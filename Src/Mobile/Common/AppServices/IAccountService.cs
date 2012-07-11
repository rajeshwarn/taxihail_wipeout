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

        Account GetAccount(string email, string password, out string error);

        Account UpdateUser(Account data);
		
		void EnsureListLoaded();
		
		IEnumerable<ListItem> GetCompaniesList( );
		
		IEnumerable<ListItem> GetVehiclesList(  );
		
		IEnumerable<ListItem> GetPaymentsList(  );

		bool ResetPassword( ResetPasswordData data );

		void ResendConfirmationEmail(string email);
		
		bool Register (RegisterAccount data, out string error);

        Address FindInAccountAddresses(double latitude, double longitude);

        IEnumerable<Address> GetHistoryAddresses();

        IEnumerable<Address> GetFavoriteAddresses();

        void UpdateAddress(Address address);

        void DeleteAddress(Guid addressId);
    }
}

