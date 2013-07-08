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
    class given_no_company
    {
        private EventSourcingTestHelper<Company> sut;
        private Guid _companyId = Guid.NewGuid();

        [SetUp]
        public void given_no_company_setup()
        {
            this.sut = new EventSourcingTestHelper<Company>();
            this.sut.Setup(new CompanyCommandHandler(this.sut.Repository));
        }

        [Test]
        public void when_registering_company_successfully()
        {
            this.sut.When(new CreateCompany { CompanyId = _companyId});

            var @event = sut.ThenHasSingle<CompanyCreated>();

            Assert.AreEqual(1, sut.Events.Count);
            Assert.AreEqual(_companyId, @event.SourceId);
        }
    }
}
