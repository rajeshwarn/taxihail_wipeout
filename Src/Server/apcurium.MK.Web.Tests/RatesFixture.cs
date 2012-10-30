using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class RatesFixture: BaseTest
    {
        private Guid _knownRateId;

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
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);
            sut.CreateRate(new Rates
            {
                Id = (_knownRateId = Guid.NewGuid()),
                Type = RateType.Recurring,
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
        public void AddRate()
        {
            var rateId = Guid.NewGuid();
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);

            sut.CreateRate(new Rates
            {
                Id = rateId,
                Type = RateType.Recurring,
                DaysOfTheWeek = DayOfTheWeek.Sunday,
                StartTime = DateTime.MinValue.AddHours(2),
                EndTime = DateTime.MinValue.AddHours(3),
                Name = "Rate " + rateId.ToString(),
                DistanceMultiplicator = 1.1,
                FlatRate = 1.2m,
                PricePerPassenger = 1.3m,
                TimeAdjustmentFactor = 1.4
            });

            var rates = sut.GetRates();

            Assert.AreEqual(1, rates.Count(x => x.Id == rateId));
            var rate = rates.Single(x => x.Id == rateId);
            Assert.AreEqual(DayOfTheWeek.Sunday, rate.DaysOfTheWeek);
            Assert.AreEqual("Rate " + rateId.ToString(), rate.Name);
            Assert.AreEqual(2, rate.StartTime.Hour);
            Assert.AreEqual(3, rate.EndTime.Hour);
            Assert.AreEqual(1.1, rate.DistanceMultiplicator);
            Assert.AreEqual(1.2m, rate.FlatRate);
            Assert.AreEqual(1.3m, rate.PricePerPassenger);
            Assert.AreEqual(1.4, rate.TimeAdjustmentFactor);
            
        }

        [Test]
        public void DeleteRate()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);

            sut.DeleteRate(_knownRateId);

            var rates = sut.GetRates();

            Assert.IsFalse(rates.Any(x=> x.Id == _knownRateId));
        }
    }
}
