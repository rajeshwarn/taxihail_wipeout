using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Infrastructure;

namespace TaxiMobile.Lib.Tests
{
    public class TestAppContext : IAppContext   
    {
        private AccountData _accountData = new AccountData();

        public AccountData LoggedUser
        {
            get{ return _accountData; }
        }
    }
}