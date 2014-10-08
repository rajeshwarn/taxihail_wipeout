﻿#region

using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

#endregion

namespace MK.Booking.IBS.Test.StaticDataWebServiceClientFixture
{
    [TestFixture]
    public class given_a_service_provider
    {
        private const int TheChauffeurGroupProviderId = 18;

        [Test]
        public void get_vehicles_test()
        {
            var sut = new StaticDataWebServiceClient(new FakeServerSettings(), new Logger());

            var vehicles = sut.GetVehiclesList(new ListItem {Id = TheChauffeurGroupProviderId});

            Assert.Greater(vehicles.Length, 0);
        }
    }
}