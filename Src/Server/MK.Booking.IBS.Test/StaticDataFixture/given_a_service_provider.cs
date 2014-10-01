#region

using NUnit.Framework;

#endregion

namespace MK.Booking.IBS.Test.StaticDataFixture
{
    [TestFixture]
    public class given_a_service_provider
    {
        [SetUp]
        public void Setup()
        {
            Sut = new StaticDataserviceEx
            {
                Url = "http://apcuriumibs:6928/XDS_IASPI.DLL/soap/IStaticData"
            };
        }

        private const int MobileKnowledgeProviderId = 18;
        protected StaticDataserviceEx Sut { get; private set; }


        [Test]
        public void when_no_zone_exists_for_coordinates()
        {
            var zone = Sut.GetCompanyZoneByGPS("taxi", "test", MobileKnowledgeProviderId, 43.0, -78);

            Assert.AreEqual(" ", zone);
        }

        [Test]
        public void when_zone_exists_for_coordinates()
        {
            var zone = Sut.GetCompanyZoneByGPS("taxi", "test", MobileKnowledgeProviderId, 43.566900, -79.574300);

            Assert.AreEqual("  4", zone);
        }
    }
}