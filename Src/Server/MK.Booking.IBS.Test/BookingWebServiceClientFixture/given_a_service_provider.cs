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
            IServerSettings config = new FakeServerSettings();
            ILogger logger = new Logger();

            var profile = new IbsAutoMapperProfile();
            Mapper.AddProfile(profile);

            var service = new BookingWebServiceClient(config, logger);
            
            var nbOfAvailableCars = service.GetAvailableVehicles(45.498247, -73.656673, 1).ToList();
            Console.WriteLine(nbOfAvailableCars.Count);

            // IBS Fare Estimate
            var fareEstimate = service.GetFareEstimate(45.498247, -73.656673, 45.4987, -73.658, null, null, null, null, null, null, null, 0);
            Console.WriteLine(fareEstimate.Distance);
            Console.WriteLine(fareEstimate.FareEstimate);
            Console.WriteLine(fareEstimate.Tolls);
        }
    }
}