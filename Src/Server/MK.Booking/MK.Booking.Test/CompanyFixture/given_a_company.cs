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
    public class given_a_company
    {
        private EventSourcingTestHelper<Company> sut;
        readonly Guid _companyId = Guid.NewGuid();

        [SetUp]
        public void given_a_company_setup()
        {
            this.sut = new EventSourcingTestHelper<Company>();
            this.sut.Setup(new CompanyCommandHandler(this.sut.Repository));
            this.sut.Given(new CompanyCreated{ SourceId = _companyId});
        }

        [Test]
        public void when_creating_a_new_rate()
        {
            var rateId = Guid.NewGuid();
            this.sut.When(new CreateRate { CompanyId = _companyId, RateId = rateId, FlatRate = 3.50, });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (RateCreated) sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(rateId, evt.RateId);
            Assert.AreEqual(3.50, evt.FlatRate);

        }
    }
}
