using TaxiMobile.Lib.Data;

namespace TaxiMobile.Lib.Services
{
	public interface IAccountService
	{
		AccountData GetAccount( string email, string password , out string  error);
			
		AccountData UpdateUser( AccountData data );
		
		bool ResetPassword( ResetPasswordData data );
		
		bool CreateAccount( CreateAccountData data, out string error);
	}
}

