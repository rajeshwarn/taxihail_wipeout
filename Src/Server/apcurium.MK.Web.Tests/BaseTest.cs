using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.SelfHost;

namespace apcurium.MK.Web.Tests
{
    public class BaseTest
    {
        private AppHost _appHost;
        protected string BaseUrl { get { return "http://localhost:6901/"; }}

        protected Account TestAccount { get; set; }
        protected string TestAccountPassword { get { return "password1"; } }

        protected void Setup()
        {
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);

            _appHost = new AppHost();
            _appHost.Init();
            _appHost.Start(BaseUrl);

            var sut = new AccountServiceClient(BaseUrl, null);
            TestAccount = sut.GetTestAccount(1);            
        }

        protected void TearDown()
        {
            _appHost.Stop();
        }
    }
}
