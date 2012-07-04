using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Web.Tests
{
    public class BaseTest
    {
        protected string BaseUrl { get { return "http://localhost.:6900/api"; }}

        protected Account TestAccount { get; set; }
        protected string TestAccountPassword { get { return "password1"; } }

        protected void Setup()
        {
            var sut = new AccountServiceClient(BaseUrl, null);
            TestAccount = sut.GetTestAccount(0);            
        }
    }
}
