using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.MapDataProvider.Google;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    public class given_an_email_address_and_an_orderStatus_using_real_geocoding_with_corporate_key : given_a_read_model_database_for_receipt_email_map
    {
        [SetUp]
        public void Setup()
        {
            base.Setup();

            // this is to test the GoogleMapKey setting of a company
            ConfigurationManager.SetSetting("GoogleMapKey", "");

            var notificationService = new NotificationService(() => new BookingDbContext(DbName),
                null,
                TemplateServiceMock.Object,
                EmailSenderMock.Object,
                ConfigurationManager,
                new ConfigurationDao(() => new ConfigurationDbContext(DbName)),
                new OrderDao(() => new BookingDbContext(DbName)),
                new AccountDao(() => new BookingDbContext(DbName)), 
                new StaticMap(),
                null,
                new Geocoding(new GoogleApiClient(ConfigurationManager, new Logger(), null), ConfigurationManager, null, new Logger()),
                null,
                null);
            notificationService.SetBaseUrl(new Uri("http://www.example.net"));

            Sut.Setup(new EmailCommandHandler(notificationService));
        }

        [Test]
        [Ignore("Needs a valid GoogleMap directions api key")]
        public void when_sending_receipt_with_no_dropoff_should_geocode_vehicle_position()
        {
            var orderId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleLatitude = 45.531608,
                    VehicleLongitude = -73.622791,
                    PickupDate = DateTime.Now,
                });
            }

            Sut.When(new SendReceipt
            {
                OrderId = orderId,
                EmailAddress = "test2@example.net",
                PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4",
                    Latitude = 1.23456,
                    Longitude = 7.890123
                },
                ClientLanguageCode = "fr"
            });

            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, "DropOffAddress", "7250 Rue du Mile End, Montral, QC H2R 2W1"))), Times.Exactly(2));
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyContains(o, "StaticMapUri", "?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C45.531608,-73.622791"))), Times.Exactly(2));
        }

        [Test]
        [Ignore("Needs a valid GoogleMap directions api key")]
        public void when_sending_receipt_with_dropoff_and_vehicle_position_should_geocode_vehicle_position()
        {
            var orderId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleLatitude = 45.531608,
                    VehicleLongitude = -73.622791,
                    PickupDate = DateTime.Now,
                });
            }

            Sut.When(new SendReceipt
            {
                OrderId = orderId,
                EmailAddress = "test2@example.net",
                PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4",
                    Latitude = 1.23456,
                    Longitude = 7.890123
                },
                DropOffAddress = new Address
                {
                    FullAddress = "hardcoded dropoff",
                    Latitude = 9.123,
                    Longitude = 6.124
                },
                ClientLanguageCode = "fr"
            });

            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, "DropOffAddress", "7250 Rue du Mile End, Montral, QC H2R 2W1"))), Times.Exactly(2));
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyContains(o, "StaticMapUri","?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C45.531608,-73.622791"))), Times.Exactly(2));
        }
    }
}