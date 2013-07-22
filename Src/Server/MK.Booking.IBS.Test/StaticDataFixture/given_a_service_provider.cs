﻿using NUnit.Framework;

namespace MK.Booking.IBS.Test.StaticDataFixture
{
    [TestFixture]
    public class given_a_service_provider
    {
        private const int TheChauffeurGroupProviderId = 17;
        private const int MobileKnowledgeProviderId = 18;
        protected StaticDataservice Sut { get; private set; }

        [SetUp]
        public void Setup()
        {
            Sut = new StaticDataservice
                      {
                          Url = "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/IStaticData"
                      };
        }



        [Test]
        public void when_no_zone_exists_for_coordinates()
        {
            var zone = Sut.GetCompanyZoneByGPS("taxi", "test", TheChauffeurGroupProviderId, 43.566900, -79.574300);

            Assert.AreEqual(" ", zone);
        }

        [Test]
        public void when_zone_exists_for_coordinates()
        {
            var zone = Sut.GetCompanyZoneByGPS("taxi", "test", MobileKnowledgeProviderId, 43.566900, -79.574300);

            Assert.AreEqual("701", zone);

        }
    }
}
