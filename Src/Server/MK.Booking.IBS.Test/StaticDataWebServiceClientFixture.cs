using NUnit.Framework;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;

namespace MK.Booking.IBS.Test
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
       
    }
}
