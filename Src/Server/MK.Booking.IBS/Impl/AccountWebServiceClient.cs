using System;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using System.Text.RegularExpressions;
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
            Regex regEx = new Regex(@"\D");
            string phoneClean = regEx.Replace(phone, "");

            UseService(service =>
            {
                var account = new TBookAccount3();
                account.WEBID = accountId.ToString();
                account.Address = new TWEBAddress() { };
                account.Email2 = email;
                account.Title = "";
                account.FirstName = firstName;
                account.LastName = lastName;
                account.Phone = phoneClean;
                account.MobilePhone = phoneClean;               
                account.WEBPassword = password;

                ibsAcccountId = service.SaveAccount3(UserNameApp, PasswordApp, account);
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
