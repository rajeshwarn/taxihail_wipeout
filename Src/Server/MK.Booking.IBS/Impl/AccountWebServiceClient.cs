#region

using System;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using System.Text.RegularExpressions;
#endregion

namespace apcurium.MK.Booking.IBS.Impl
{
    public class AccountWebServiceClient : BaseService<WebAccount3Service>, IAccountWebServiceClient
    {
        public AccountWebServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings.ServerData.IBS, logger)
        {
        }

        public AccountWebServiceClient(IBSSettingContainer ibsSettings, ILogger logger)
            : base(ibsSettings,logger)
        {
            
        }


        public int CreateAccount(Guid accountId, string email, string firstName, string lastName, string phone)
        {
            var password = _ibsSettings.DefaultAccountPassword;
            var isSuccess = false;
            var ibsAcccountId = 0;
            var regEx = new Regex(@"\D");
            var phoneClean = regEx.Replace(phone, "");

            UseService(service =>
            {
                var account = new TBookAccount3
                {
                    WEBID = accountId.ToString(),
                    Address = new TWEBAddress(),
                    Email2 = email,
                    Title = "",
                    FirstName = firstName,                           
                    LastName = lastName,
                    Phone = phoneClean,
                    MobilePhone = phoneClean,
                    WEBPassword = password
                };

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

        protected override string GetUrl()
        {
            return base.GetUrl() + "IWebAccount3";
        }
    }
}