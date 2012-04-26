namespace TaxiMobileApp
{
	public interface IAccountService
	{
		
		AccountData GetAccount( string email, string password , out string  error);
			
		AccountData UpdateUser( AccountData data );
		
		void EnsureListLoaded();
		
		ListItem[] GetCompaniesList( );
		
		ListItem[] GetVehiclesList(  );
		
		ListItem[] GetPaymentsList(  );
		
		bool ResetPassword( ResetPasswordData data );
		
		bool CreateAccount( CreateAccountData data, out string error);
	}
}

