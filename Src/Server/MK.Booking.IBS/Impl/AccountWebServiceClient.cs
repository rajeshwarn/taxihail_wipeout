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
        private readonly IServerSettings _serverSettings;

        public AccountWebServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings.ServerData.IBS, logger)
        {
            _serverSettings = serverSettings;
        }

        public AccountWebServiceClient(IServerSettings serverSettings, IBSSettingContainer ibsSettings, ILogger logger)
            : base(ibsSettings, logger)
        {
            // for now, server settings is for the home server, so if one day we want a real roaming mode (not network),
            // this will need to be changed

            _serverSettings = serverSettings;
        }

        public int CreateAccount(Guid accountId, string email, string firstName, string lastName, string phone)
        {
            // we don't care about the ibs password, it's never used, but create an account 
            // using the home password since anyway the ibs account will not be used under another company
            var password = _serverSettings.ServerData.IBS.DefaultAccountPassword;
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