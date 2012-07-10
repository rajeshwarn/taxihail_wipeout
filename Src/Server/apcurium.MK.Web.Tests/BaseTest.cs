using System.Data.Entity;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.SelfHost;

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
    }
}
