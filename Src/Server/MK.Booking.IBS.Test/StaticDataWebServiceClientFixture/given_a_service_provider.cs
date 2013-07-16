using NUnit.Framework;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;

namespace MK.Booking.IBS.Test.StaticDataWebServiceClientFixture
{
    [TestFixture]
    public class given_a_service_provider
    {
        private const int TheChauffeurGroupProviderId = 17;
        private const int MobileKnowledgeProviderId = 18;

        [Test]
        public void when_no_zone_exists_for_coordinates()
        {
            var sut = new StaticDataWebServiceClient(new FakeConfigurationManager(), new Logger());

            var zone = sut.GetZoneByCoordinate(TheChauffeurGroupProviderId, 43.566900, -79.574300);

            Assert.AreEqual(" ", zone);
        }

        [Test]
        public void when_zone_exists_for_coordinates()
        {
            var sut = new StaticDataWebServiceClient(new FakeConfigurationManager(), new Logger());

            var zone = sut.GetZoneByCoordinate(MobileKnowledgeProviderId, 43.566900, -79.574300);

            Assert.AreEqual("701", zone);
        }

        [Test]
        public void get_vehicles_test()
        {
            var sut = new StaticDataWebServiceClient(new FakeConfigurationManager(), new Logger());

            var vehicles = sut.GetVehiclesList(new ListItem { Id = TheChauffeurGroupProviderId });

            Assert.Inconclusive();
        }
        
        [Test]
        public void get_pickup_cities_test()
        {
            var sut = new StaticDataWebServiceClient(new FakeConfigurationManager(), new Logger());

            var cities = sut.GetPickupCity(new ListItem { Id = MobileKnowledgeProviderId });

            Assert.Inconclusive();
        }

        [Test]
        public void get_payments_test()
        {
            var sut = new StaticDataWebServiceClient(new FakeConfigurationManager(), new Logger());

            var payments = sut.GetPaymentsList(new ListItem { Id = MobileKnowledgeProviderId });

            Assert.Inconclusive();
        }

        [Test]
        public void StaticDataservice_test()
        {
            var service = new StaticDataservice();
            service.Url = "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/IStaticData";

            var cities = service.GetDropoffCityList("taxi", "test", 17);

            Assert.Inconclusive();
        }

        [Test]
        public void get_dropoff_cities_test()
        {
            var sut = new StaticDataWebServiceClient(new FakeConfigurationManager(), new Logger());

            var cities = sut.GetDropoffCity(new ListItem { Id = MobileKnowledgeProviderId });

            Assert.Inconclusive();
        }
       
    }
}
