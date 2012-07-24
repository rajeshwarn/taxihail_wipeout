using System.Data.Entity;
using System.IO;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.SelfHost;
using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using log4net.Config;

namespace apcurium.MK.Web.Tests
{
    public class BaseTest
    {
        static readonly AppHost _appHost;

        //staging url : http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/
        protected string BaseUrl { get { return "http://localhost:6901/"; } }

        protected Account TestAccount { get; set; }
        protected string TestAccountPassword { get { return "password1"; } }

        static BaseTest()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.xml"));
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);
            _appHost = new AppHost();
            _appHost.Init();
        }

        protected void Setup()
        {
            _appHost.Start(BaseUrl);

            var sut = new AccountServiceClient(BaseUrl, null);
            TestAccount = sut.GetTestAccount(0);            
        }

        protected void TearDown()
        {
            _appHost.Stop();
        }

        protected  string GetTempEmail()
        {
            var email = string.Format("testemail.{0}@apcurium.com", Guid.NewGuid().ToString().Replace("-", ""));
            return email;
        }

        protected Account GetNewAccount()
        {
            var accountService = new AccountServiceClient(BaseUrl, null);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", Password = "password" };
            accountService.RegisterAccount(newAccount);

            accountService = new AccountServiceClient(BaseUrl, new AuthInfo(newAccount.Email, "password"));
            
            return accountService.GetMyAccount();
        }
        
    }
}
