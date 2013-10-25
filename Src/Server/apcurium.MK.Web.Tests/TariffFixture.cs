using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using Tariff = apcurium.MK.Booking.Api.Contract.Requests.Tariff;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class TariffFixture : BaseTest
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
            var sut = new TariffsServiceClient(BaseUrl, SessionId, "Test");
            sut.CreateTariff(new Common.Entity.Tariff
            {
                Id = (_knownTariffId = Guid.NewGuid()),
                Type = (int)TariffType.Recurring,
                DaysOfTheWeek = (int)DayOfTheWeek.Sunday,
                StartTime =DateTime.MinValue.AddHours(2),
                EndTime = DateTime.MinValue.AddHours(3),
                Name = "Rate "  + Guid.NewGuid().ToString(),
                KilometricRate = 1.1,
                FlatRate = 1.2m,
                PassengerRate = 1.3m,
                MarginOfError = 1.4,
                KilometerIncluded = 0,

            });
        }

        [Test]
        public void AddTariff()
        {
            var tariffId = Guid.NewGuid();
            var sut = new TariffsServiceClient(BaseUrl, SessionId, "Test");

            sut.CreateTariff(new Common.Entity.Tariff
            {
                Id = tariffId,
                Type = (int)TariffType.Recurring,
                DaysOfTheWeek = (int)DayOfTheWeek.Sunday,
                StartTime = DateTime.Today.AddHours(2),
                EndTime = DateTime.Today.AddHours(3),
                Name = "Rate " + tariffId.ToString(),
                KilometricRate = 1.1,
                FlatRate = 1.2m,
                PassengerRate = 1.3m,
                MarginOfError = 1.4, 
                KilometerIncluded = 1.6

            });

            var rates = sut.GetTariffs();

            Assert.AreEqual(1, rates.Count(x => x.Id == tariffId));
            var rate = rates.Single(x => x.Id == tariffId);
            Assert.AreEqual((int)DayOfTheWeek.Sunday, rate.DaysOfTheWeek);
            Assert.AreEqual("Rate " + tariffId.ToString(), rate.Name);
            Assert.AreEqual(2, rate.StartTime.Hour);
            Assert.AreEqual(3, rate.EndTime.Hour);
            Assert.AreEqual(1.1, rate.KilometricRate);
            Assert.AreEqual(1.2m, rate.FlatRate);
            Assert.AreEqual(1.3m, rate.PassengerRate);
            Assert.AreEqual(1.4, rate.MarginOfError);
            Assert.AreEqual(1.6, rate.KilometerIncluded);
            
        }

        [Test]
        public void DeleteTariff()
        {
            var sut = new TariffsServiceClient(BaseUrl, SessionId, "Test");

            sut.DeleteTariff(_knownTariffId);

            var rates = sut.GetTariffs();

            Assert.IsFalse(rates.Any(x=> x.Id == _knownTariffId));
        }
    }
}
