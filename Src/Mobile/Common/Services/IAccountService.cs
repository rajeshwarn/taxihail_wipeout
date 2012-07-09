using System;
using TaxiMobileApp.Lib.IBS;


namespace TaxiMobileApp
{
	public interface IAccountService
	{
		
		AccountData GetAccount( string email, string password , out string  error, out ErrorCode errorCode );
			
		AccountData UpdateUser( AccountData data );
		
		void EnsureListLoaded();
		
		ListItem[] GetCompaniesList( );
		
		ListItem[] GetVehiclesList(  );
		
		ListItem[] GetPaymentsList(  );

		bool ResetPassword( ResetPasswordData data );

		void ResendConfirmationEmail(string email);
		
		bool CreateAccount( CreateAccountData data, out string error);
	}
}

