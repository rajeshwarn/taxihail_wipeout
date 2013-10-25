using System.Data.Entity;
using System.IO;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.SelfHost;
using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using log4net.Config;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;

namespace apcurium.MK.Web.Tests
{
    public class BaseTest
    {
        static readonly AppHost _appHost;

        //staging url : http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/
        protected string BaseUrl { get { return "http://localhost:6901/"; } }
        //protected string BaseUrl { get { return "http://192.168.12.114/apcurium.MK.Web/api/"; } }
        
        protected Account TestAccount { get; set; }
        protected Account TestAdminAccount { get; set; }
        protected string TestAdminAccountPassword { get { return "password1"; } }
        protected string TestAccountPassword { get { return "password1"; } }
        protected string SessionId { get; set; }

        protected AccountServiceClient AccountService { get { return new AccountServiceClient(BaseUrl, SessionId, "Test", GetFakePaymentClient()); } }


        protected DummyConfigManager DummyConfigManager { get { return new DummyConfigManager(); } }

        static BaseTest()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.xml"));
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);
            _appHost = new AppHost();
            _appHost.Init();
        }

        protected IPaymentServiceClient GetFakePaymentClient()
        {
            return new FakePaymentClient();
        }
        public virtual void TestFixtureSetup()
        {
            _appHost.Start(BaseUrl);

            TestAccount = AccountService.GetTestAccount(0);
        }

        public virtual void Setup()
        {
            var authResponse = new AuthServiceClient(BaseUrl, null, "Test").Authenticate(TestAccount.Email, TestAccountPassword);
            SessionId = authResponse.SessionId;
           
           
        }

        public virtual void TestFixtureTearDown()
        {
            _appHost.Stop();
        }

        protected  string GetTempEmail()
        {
            var email = string.Format("testemail.{0}@apcurium.com", Guid.NewGuid().ToString().Replace("-", ""));
            return email;
        }

        protected Account CreateAndAuthenticateTestAccount()
        {
            var newAccount = AccountService.CreateTestAccount();
            var authResponse = new AuthServiceClient(BaseUrl, null, "Test").Authenticate(newAccount.Email, TestAccountPassword);
            SessionId = authResponse.SessionId;
            return newAccount;
        }

        protected Account CreateAndAuthenticateTestAdminAccount()
        {
            var newAccount = AccountService.CreateTestAdminAccount();
            var authResponse = new AuthServiceClient(BaseUrl, null, "Test").Authenticate(newAccount.Email, TestAccountPassword);
            SessionId = authResponse.SessionId;
            return newAccount;
        }
        

        protected Account GetNewFacebookAccount()
        {
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", FacebookId = Guid.NewGuid().ToString(), Language = "en" };
            AccountService.RegisterAccount(newAccount);

            var authResponse = new AuthServiceClient(BaseUrl, null, "Test").AuthenticateFacebook(newAccount.FacebookId);
            SessionId = authResponse.SessionId;

            return AccountService.GetMyAccount();
        }

        protected Account GetNewTwitterAccount()
        {
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", TwitterId = Guid.NewGuid().ToString(), Language = "en" };
            AccountService.RegisterAccount(newAccount);

            var authResponse = new AuthServiceClient(BaseUrl, null, "Test").AuthenticateTwitter(newAccount.TwitterId);
            SessionId = authResponse.SessionId;

            return AccountService.GetMyAccount();
        }
        
    }
}
