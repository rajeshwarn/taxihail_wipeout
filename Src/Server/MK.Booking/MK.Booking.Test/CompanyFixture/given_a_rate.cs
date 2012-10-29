using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_rate
    {
        private EventSourcingTestHelper<Company> sut;
        readonly Guid _companyId = Guid.NewGuid();
        readonly Guid _rateId = Guid.NewGuid();

        [SetUp]
        public void given_a_rate_setup()
        {
            this.sut = new EventSourcingTestHelper<Company>();
            this.sut.Setup(new CompanyCommandHandler(this.sut.Repository));
            this.sut.Given(new CompanyCreated { SourceId = _companyId });
            this.sut.Given(new RateCreated { SourceId = _companyId });
        }

        [Test]
        public void when_updating_rate()
        {
            sut.When(new UpdateRate
                         {
                             CompanyId = _companyId,
                             RateId = _rateId,
                             FlatRate = 12m,
                             PricePerPassenger = 13,
                             DistanceMultiplicator = 14,
                             TimeAdjustmentFactor = 15,
                             StartTime = DateTime.Today.AddHours(16),
                             EndTime = DateTime.Today.AddHours(17),
                             Name = "Updated",
                             DaysOfTheWeek = DayOfTheWeek.Wednesday
                         });

            Assert.AreEqual(1, sut.Events.Count);
            Assert.IsInstanceOf<RateUpdated>(sut.Events.Single());
            var evt = (RateUpdated)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_rateId, evt.RateId);
            Assert.AreEqual(12, evt.FlatRate);
            Assert.AreEqual(13, evt.PricePerPassenger);
            Assert.AreEqual(14, evt.DistanceMultiplicator);
            Assert.AreEqual(15, evt.TimeAdjustmentFactor);
            Assert.AreEqual("Updated", evt.Name);
            Assert.AreEqual(DayOfTheWeek.Wednesday, evt.DaysOfTheWeek);
            Assert.AreEqual(DateTime.Today.AddHours(16), evt.StartTime);
            Assert.AreEqual(DateTime.Today.AddHours(17), evt.EndTime);

        }

        [Test]
        public void when_deleting_rate()
        {
            sut.When(new DeleteRate{ CompanyId = _companyId, RateId = _rateId});
            Assert.AreEqual(1, sut.Events.Count);
            Assert.IsInstanceOf<RateDeleted>(sut.Events.Single());
            var evt = (RateDeleted)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_rateId, evt.RateId);
        }
    }
}
