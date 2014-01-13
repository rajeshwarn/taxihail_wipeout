using System.IO;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Fake;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common;
using apcurium.MK.Web.SelfHost;
using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using log4net.Config;

namespace apcurium.MK.Web.Tests
{
    public class BaseTest
    {
        static readonly AppHost AppHost;
        protected string BaseUrl { get { return "http://localhost:6901/"; } }
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
            AppHost = new AppHost();
            AppHost.Init();
        }

        protected IPaymentServiceClient GetFakePaymentClient()
        {
            return new FakePaymentClient();
        }

        public virtual void TestFixtureSetup()
        {
            AppHost.Start(BaseUrl);
            var task = AccountService.GetTestAccount(0);
            task.Wait();
            TestAccount = task.Result;
        }

        public virtual void Setup()
        {
            var authResponseTask = new AuthServiceClient(BaseUrl, null, "Test").Authenticate(TestAccount.Email, TestAccountPassword);
            authResponseTask.Wait();
            SessionId = authResponseTask.Result.SessionId;
        }

        public virtual void TestFixtureTearDown()
        {
            AppHost.Stop();
        }

        protected  string GetTempEmail()
        {
            return string.Format("testemail.{0}@apcurium.com", Guid.NewGuid().ToString().Replace("-", ""));
        }

        protected async Task<Account> CreateAndAuthenticateTestAccount()
        {
            var newAccount = await AccountService.CreateTestAccount();
            var authResponse = await new AuthServiceClient(BaseUrl, null, "Test").Authenticate(newAccount.Email, TestAccountPassword);
            SessionId = authResponse.SessionId;
            return newAccount;
        }

        protected async Task<Account> CreateAndAuthenticateTestAdminAccount()
        {
            var newAccount = await AccountService.CreateTestAdminAccount();
            var authResponse = await new AuthServiceClient(BaseUrl, null, "Test").Authenticate(newAccount.Email, TestAccountPassword);
            SessionId = authResponse.SessionId;
            return newAccount;
        }
        
        protected async Task<Account> GetNewFacebookAccount()
        {
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", FacebookId = Guid.NewGuid().ToString(), Language = "en" };
            await AccountService.RegisterAccount(newAccount);

            var client = new AuthServiceClient(BaseUrl, null, "Test");
            var authResponse = await client.AuthenticateFacebook(newAccount.FacebookId);
            SessionId = authResponse.SessionId;

            return await AccountService.GetMyAccount();
        }

        protected async Task<Account> GetNewTwitterAccount()
        {
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone = "5146543024", Email = GetTempEmail(), Name = "First Name Test", TwitterId = Guid.NewGuid().ToString(), Language = "en" };
            await AccountService.RegisterAccount(newAccount);

            var authResponse = await new AuthServiceClient(BaseUrl, null, "Test").AuthenticateTwitter(newAccount.TwitterId);
            SessionId = authResponse.SessionId;

            return await AccountService.GetMyAccount();
        }
    }
}
