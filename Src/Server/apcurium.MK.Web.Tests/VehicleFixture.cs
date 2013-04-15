using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Web.Tests
{
    public class VehicleFixture : BaseTest
    {
        [Test]
        public void sending_message_to_driver()
        {
            var client = new VehicleServiceClient(BaseUrl, SessionId);

            var x = client.SendMessage("1234", "Hello");
        }
    }
}
