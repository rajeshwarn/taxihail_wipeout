#region

using System;
using System.Linq;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using AutoMapper;
using NUnit.Framework;

#endregion

namespace MK.Booking.IBS.Test.BookingWebServiceClientFixture
{
    public class given_a_service_provider
    {
        [Test]
        public void StaticDataservice_test()
        {
            IConfigurationManager config = new FakeConfigurationManager();
            ILogger logger = new Logger();
            IStaticDataWebServiceClient webServiceClient = new StaticDataWebServiceClient(config, logger);

            var profile = new IbsAutoMapperProfile();
            Mapper.AddProfile(profile);

            var service = new BookingWebServiceClient(config, logger, webServiceClient);

            var test = service.GetAvailableVehicles(45.498247, -73.656673, 1).ToList();

            Console.WriteLine(test.Count);
        }
    }
}