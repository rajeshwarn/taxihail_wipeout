#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_no_company
    {
        [SetUp]
        public void given_no_company_setup()
        {
            _sut = new EventSourcingTestHelper<Company>();
            _sut.Setup(new CompanyCommandHandler(_sut.Repository));
        }

        private EventSourcingTestHelper<Company> _sut;
        private readonly Guid _companyId = Guid.NewGuid();

        [Test]
        public void when_registering_company_successfully()
        {
            _sut.When(new CreateCompany {CompanyId = _companyId});

            var @event = _sut.ThenHasSingle<CompanyCreated>();

            Assert.AreEqual(1, _sut.Events.Count);
            Assert.AreEqual(_companyId, @event.SourceId);
        }
    }
}