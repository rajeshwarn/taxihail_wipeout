using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Data;


namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAccountService
	{
		
		AccountData GetAccount( string email, string password , out string  error );
			
		AccountData UpdateUser( AccountData data );
		
		void EnsureListLoaded();
		
		IEnumerable<ListItem> GetCompaniesList( );
		
		IEnumerable<ListItem> GetVehiclesList(  );
		
		IEnumerable<ListItem> GetPaymentsList(  );

		bool ResetPassword( ResetPasswordData data );

		void ResendConfirmationEmail(string email);
		
		bool CreateAccount( CreateAccountData data, out string error);

        LocationData[] GetHistoryAddresses();

        LocationData[] GetFavoriteAddresses();
    }
}

