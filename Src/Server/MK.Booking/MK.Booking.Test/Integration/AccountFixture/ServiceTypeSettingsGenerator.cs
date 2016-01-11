using System;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using MK.Common.Configuration;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
    public class given_a_servicetypesettings_model_generator : given_a_read_model_database
    {
        protected ServiceTypeSettingsGenerator Sut;

        public given_a_servicetypesettings_model_generator()
        {
            Sut = new ServiceTypeSettingsGenerator(() => new ConfigurationDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_servicetype_settings : given_a_servicetypesettings_model_generator
    {
        [Test]
        public void when_settings_dont_exist_and_settings_updated_then_dto_populated()
        {
            var companyId = Guid.NewGuid();

            Sut.Handle(new ServiceTypeSettingsUpdated
            {
                SourceId = companyId,
                ServiceTypeSettings = new ServiceTypeSettings
                {
                    ServiceType = ServiceType.Luxury,
                    ProviderId = 0,
                    IBSWebServicesUrl = "test"
                }
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var dto = context.ServiceTypeSettings.Find(ServiceType.Luxury);

                Assert.NotNull(dto);
                Assert.AreEqual(ServiceType.Luxury, dto.ServiceType);
                Assert.AreEqual(0, dto.ProviderId);
                Assert.AreEqual("test", dto.IBSWebServicesUrl);
            }
        }
    }

    [TestFixture]
    public class given_a_servicetype_settings : given_a_servicetypesettings_model_generator
    {
        private Guid _companyId = Guid.NewGuid();

        public given_a_servicetype_settings()
        {
            Sut.Handle(new ServiceTypeSettingsUpdated
            {
                SourceId = _companyId,
                ServiceTypeSettings = new ServiceTypeSettings
                {
                    ServiceType = ServiceType.Luxury,
                    ProviderId = 0,
                    IBSWebServicesUrl = "test"
                }
            });
        }

        [Test]
        public void when_settings_exist_and_settings_updated_then_dto_updated()
        {
            Sut.Handle(new ServiceTypeSettingsUpdated
            {
                SourceId = _companyId,
                ServiceTypeSettings = new ServiceTypeSettings
                {
                    ServiceType = ServiceType.Luxury,
                    ProviderId = 0,
                    IBSWebServicesUrl = "test2"
                }
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var dto = context.ServiceTypeSettings.Find(ServiceType.Luxury);

                Assert.NotNull(dto);
                Assert.AreEqual(ServiceType.Luxury, dto.ServiceType);
                Assert.AreEqual(0, dto.ProviderId);
                Assert.AreEqual("test2", dto.IBSWebServicesUrl);
            }
        }
    }
}