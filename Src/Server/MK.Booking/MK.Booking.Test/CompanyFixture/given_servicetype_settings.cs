using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Enumeration;
using MK.Common.Configuration;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_servicetype_settings
    {
        private EventSourcingTestHelper<Company> _sut;
        private readonly Guid _companyId = AppConstants.CompanyId;

        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Company>();
            _sut.Setup(new CompanyCommandHandler(_sut.Repository, null));
            _sut.Given(new CompanyCreated { SourceId = _companyId });
        }

        [Test]
        public void when_updating_servicetype_settings()
        {
            _sut.When(new UpdateServiceTypeSettings
            {
                CompanyId = _companyId,
                ServiceTypeSettings = new ServiceTypeSettings
                {
                    ServiceType = ServiceType.Luxury,
                    IBSWebServicesUrl = "test",
                    FutureBookingThresholdInMinutes = 15,
                    WaitTimeRatePerMinute = 2,
                    AirportMeetAndGreetRate = 10
                }
            });

            var evt = _sut.ThenHasSingle<ServiceTypeSettingsUpdated>();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(ServiceType.Luxury, evt.ServiceTypeSettings.ServiceType);
            Assert.AreEqual("test", evt.ServiceTypeSettings.IBSWebServicesUrl);
            Assert.AreEqual(15, evt.ServiceTypeSettings.FutureBookingThresholdInMinutes);
            Assert.AreEqual(2, evt.ServiceTypeSettings.WaitTimeRatePerMinute);
            Assert.AreEqual(10, evt.ServiceTypeSettings.AirportMeetAndGreetRate);
        }
    }
}