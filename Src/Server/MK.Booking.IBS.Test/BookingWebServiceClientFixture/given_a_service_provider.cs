using System;
using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Diagnostic;

namespace MK.Booking.IBS.Test.BookingWebServiceClientFixture
{
    public class given_a_service_provider
    {
        [Test]
        public void StaticDataservice_test()
        {
            var service = new BookingWebServiceClient(new FakeConfigurationManager(), new Logger(), null);

            var test = service.GetAvailableVehicles(45.498247, -73.656673).ToList();

            Console.WriteLine(test.Count);
        } 
    }
}