using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class TariffFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
            var sut = new TariffsServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
            sut.CreateTariff(new Tariff
                {
                    Id = (_knownTariffId = Guid.NewGuid()),
                    Type = (int) TariffType.Recurring,
                    DaysOfTheWeek = (int) DayOfTheWeek.Sunday,
                    StartTime = DateTime.MinValue.AddHours(2),
                    EndTime = DateTime.MinValue.AddHours(3),
                    Name = "Rate " + Guid.NewGuid(),
                    KilometricRate = 1.1,
                    FlatRate = 1.2m,
                    MarginOfError = 1.4,
                    PerMinuteRate = 0,
                    KilometerIncluded = 0,
                }).Wait();
        }

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

        [Test]
        public async void AddTariff()
        {
            var tariffId = Guid.NewGuid();
            var sut = new TariffsServiceClient(BaseUrl, SessionId, new DummyPackageInfo());

            await sut.CreateTariff(new Tariff
                {
                    Id = tariffId,
                    Type = (int) TariffType.Recurring,
                    DaysOfTheWeek = (int) DayOfTheWeek.Sunday,
                    StartTime = DateTime.Today.AddHours(2),
                    EndTime = DateTime.Today.AddHours(3),
                    Name = "Rate " + tariffId,
                    KilometricRate = 1.1,
                    FlatRate = 1.2m,
                    MarginOfError = 1.4,
                    PerMinuteRate = 1.5,
                    KilometerIncluded = 1.6
                });

            var rates = await sut.GetTariffs();

            Assert.AreEqual(1, rates.Count(x => x.Id == tariffId));
            var rate = rates.Single(x => x.Id == tariffId);
            Assert.AreEqual((int) DayOfTheWeek.Sunday, rate.DaysOfTheWeek);
            Assert.AreEqual("Rate " + tariffId, rate.Name);
            Assert.AreEqual(2, rate.StartTime.Hour);
            Assert.AreEqual(3, rate.EndTime.Hour);
            Assert.AreEqual(1.1, rate.KilometricRate);
            Assert.AreEqual(1.2m, rate.FlatRate);
            Assert.AreEqual(1.4, rate.MarginOfError);
            Assert.AreEqual(1.5, rate.PerMinuteRate);
            Assert.AreEqual(1.6, rate.KilometerIncluded);
            Assert.AreEqual(null, rate.VehicleTypeId);
        }

        [Test]
        public async void AddVehiculeTariff()
        {
            var tariffId = Guid.NewGuid();
            var sut = new TariffsServiceClient(BaseUrl, SessionId, new DummyPackageInfo());

            await sut.CreateTariff(new Tariff
            {
                Id = tariffId,
                Type = (int)TariffType.Vehicle,
                Name = "Rate " + tariffId,
                KilometricRate = 1.1,
                FlatRate = 1.2m,
                MarginOfError = 1.4,
                PerMinuteRate = 1.5,
                KilometerIncluded = 1.6,
                VehicleTypeId = 10
            });

            var rates = await sut.GetTariffs();

            Assert.AreEqual(1, rates.Count(x => x.Id == tariffId));
            var rate = rates.Single(x => x.Id == tariffId);
            Assert.AreEqual(3, rate.Type);
            Assert.AreEqual("Rate " + tariffId, rate.Name);
            Assert.AreEqual(1.1, rate.KilometricRate);
            Assert.AreEqual(1.2m, rate.FlatRate);
            Assert.AreEqual(1.4, rate.MarginOfError);
            Assert.AreEqual(1.5, rate.PerMinuteRate);
            Assert.AreEqual(1.6, rate.KilometerIncluded);
            Assert.AreEqual(10, rate.VehicleTypeId);
        }

        [Test]
        public async void DeleteTariff()
        {
            var sut = new TariffsServiceClient(BaseUrl, SessionId, new DummyPackageInfo());

            await sut.DeleteTariff(_knownTariffId);

            var rates = await sut.GetTariffs();

            Assert.IsFalse(rates.Any(x => x.Id == _knownTariffId));
        }
    }
}