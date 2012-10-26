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
