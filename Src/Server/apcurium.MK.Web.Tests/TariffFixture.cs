using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using Tariff = apcurium.MK.Booking.Api.Contract.Requests.Tariff;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class TariffFixture: BaseTest
    {
        private Guid _knownTariffId;

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount();
            var sut = new TariffsServiceClient(BaseUrl, SessionId);
            sut.CreateTariff(new Tariff
            {
                Id = (_knownTariffId = Guid.NewGuid()),
                Type = TariffType.Recurring,
                DaysOfTheWeek = DayOfTheWeek.Sunday,
                StartTime =DateTime.MinValue.AddHours(2),
                EndTime = DateTime.MinValue.AddHours(3),
                Name = "Rate "  + Guid.NewGuid().ToString(),
                DistanceMultiplicator = 1.1,
                FlatRate = 1.2m,
                PricePerPassenger = 1.3m,
                TimeAdjustmentFactor = 1.4
            });
        }

        [Test]
        public void AddTariff()
        {
            var tariffId = Guid.NewGuid();
            var sut = new TariffsServiceClient(BaseUrl, SessionId);

            sut.CreateTariff(new Tariff
            {
                Id = tariffId,
                Type = TariffType.Recurring,
                DaysOfTheWeek = DayOfTheWeek.Sunday,
                StartTime = DateTime.MinValue.AddHours(2),
                EndTime = DateTime.MinValue.AddHours(3),
                Name = "Rate " + tariffId.ToString(),
                DistanceMultiplicator = 1.1,
                FlatRate = 1.2m,
                PricePerPassenger = 1.3m,
                TimeAdjustmentFactor = 1.4
            });

            var rates = sut.GetTariffs();

            Assert.AreEqual(1, rates.Count(x => x.Id == tariffId));
            var rate = rates.Single(x => x.Id == tariffId);
            Assert.AreEqual((int)DayOfTheWeek.Sunday, rate.DaysOfTheWeek);
            Assert.AreEqual("Rate " + tariffId.ToString(), rate.Name);
            Assert.AreEqual(2, rate.StartTime.Hour);
            Assert.AreEqual(3, rate.EndTime.Hour);
            Assert.AreEqual(1.1, rate.DistanceMultiplicator);
            Assert.AreEqual(1.2m, rate.FlatRate);
            Assert.AreEqual(1.3m, rate.PricePerPassenger);
            Assert.AreEqual(1.4, rate.TimeAdjustmentFactor);
            
        }

        [Test]
        public void DeleteTariff()
        {
            var sut = new TariffsServiceClient(BaseUrl, SessionId);

            sut.DeleteTariff(_knownTariffId);

            var rates = sut.GetTariffs();

            Assert.IsFalse(rates.Any(x=> x.Id == _knownTariffId));
        }
    }
}
