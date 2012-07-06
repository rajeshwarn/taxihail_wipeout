using System;
using apcurium.MK.Booking.IBS.WebServices;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
namespace apcurium.MK.Booking.IBS.Impl
{
    public class AccountWebServiceClient : BaseService<WebAccount3Service>, IAccountWebServiceClient
    {
        protected override string GetUrl()
        {
            return base.GetUrl() + "IWebAccount3";
        }

        public AccountWebServiceClient(IConfigurationManager configManager, ILogger logger) : base( configManager, logger )
        {

        }
        public int CreateAccount( Guid accountId, string email, string firstName, string lastName, string phone)
        {
            string password = ConfigManager.GetSetting("IBS.DefaultAccountPassword");
            bool isSuccess = false;
            int ibsAcccountId = 0;
            UseService(service =>
            {
                var account = new TBookAccount3();
                account.WEBID = accountId.ToString();
                account.Address = new TWEBAddress() { };
                account.Email2 = email;
                account.Title = "";
                account.FirstName = firstName;
                account.LastName = lastName;
                account.Phone = phone;
                account.MobilePhone = phone;                
                account.WEBPassword = password;

                ibsAcccountId = service.SaveAccount3(_userNameApp, _passwordApp, account);
                isSuccess = ibsAcccountId > 0;
            });

            if (!isSuccess)
            {
                //TODO use valid exception
                throw new InvalidOperationException();
            }
            return ibsAcccountId;
        }
    }
}
