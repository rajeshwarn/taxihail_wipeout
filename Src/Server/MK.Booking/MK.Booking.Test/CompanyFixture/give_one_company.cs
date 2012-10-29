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
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class give_one_company
    {
        private EventSourcingTestHelper<Company> sut;
        private readonly Guid _companyId = AppConstants.CompanyId;

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Company>();
            this.sut.Setup(new CompanyCommandHandler(this.sut.Repository));
            this.sut.Given(new CompanyCreated(){SourceId = _companyId} );
            this.sut.Given(new AppSettingsAdded(){Key = "Key.Default", Value = "Value.Default"});
        }

        [Test]
        public void when_appsettings_added_successfully()
        {
            this.sut.When(new AddAppSettings() { Key = "Key.hi", Value = "Value.hi"});

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (AppSettingsAdded)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual("Key.hi", evt.Key);
            Assert.AreEqual("Value.hi", evt.Value);
        }

        [Test]
        public void when_appsettings_updated_successfully()
        {
            this.sut.When(new UpdateAppSettings() { Key = "Key.Default", Value = "Value.newValue" });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (AppSettingsUpdated)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual("Key.Default", evt.Key);
            Assert.AreEqual("Value.newValue", evt.Value);

        }
    }
}
