#region

using NUnit.Framework;

#endregion

namespace MK.Booking.IBS.Test.StaticDataFixture
{
    [TestFixture]
    [Ignore("Ignoring this test, we will need a new test ibs with test data to make this work")]
    public class given_a_service_provider
    {
        [SetUp]
        public void Setup()
        {
            Sut = new StaticDataservice
            {
                Url = "http://apcurium.drivelinq.com:16928/IBSCab/IBSCab.dll/soap//IStaticData"
            };
        }

        private const int MobileKnowledgeProviderId = 18;
        protected StaticDataservice Sut { get; private set; }


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