using System;
using System.Web.Routing;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    public class given_an_email_address_and_an_orderStatus : given_a_read_model_database_for_receipt_email_map
    {
        private Mock<IGeocoding> _geocodingMock;

        [SetUp]
        public void Setup()
        {
            base.Setup();

            _geocodingMock = new Mock<IGeocoding>();
            var notificationService = new NotificationService(() => new BookingDbContext(DbName),
                null,
                TemplateServiceMock.Object,
                EmailSenderMock.Object,
                ConfigurationManager,
                ConfigurationManager,
                new ConfigurationDao(() => new ConfigurationDbContext(DbName)),
                new OrderDao(() => new BookingDbContext(DbName)), 
                new StaticMap(),
                null,
                _geocodingMock.Object,
                null);
            notificationService.SetBaseUrl(new Uri("http://www.example.net"));

            Sut.Setup(new EmailCommandHandler(notificationService));

            _geocodingMock
                .Setup(x => x.Search(45, -73, It.IsAny<string>(), It.IsAny<GeoResult>(), false))
                .Returns(new [] {new Address { FullAddress = "full dropoff" }});
        }

        [Test]
        public void when_sending_receipt_with_no_dropoff_should_geocode_vehicle_position()
        {
            var orderId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleLatitude = 45,
                    VehicleLongitude = -73,
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

            // verify that geocoding is called
            _geocodingMock.Verify(g => g.Search(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Once);

            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, "DropOffAddress", "full dropoff"))), Times.Exactly(2));
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyContains(o, "StaticMapUri", "?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C45,-73"))), Times.Exactly(2));
        }

        [Test]
        public void when_sending_receipt_with_no_dropoff_and_no_vehicle_position_should_not_have_address()
        {
            var orderId = Guid.NewGuid();
            
            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new OrderStatusDetail
                {
                    OrderId = orderId,
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

            // verify that geocoding is not called
            _geocodingMock.Verify(g => g.Search(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Never);

            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, "DropOffAddress", "-"))), Times.Exactly(2));
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, "StaticMapUri", string.Empty))), Times.Exactly(2));
        }

        [Test]
        public void when_sending_receipt_with_dropoff_and_no_vehicle_position_should_have_address()
        {
            var orderId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new OrderStatusDetail
                {
                    OrderId = orderId,
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

            // verify that geocoding is not called
            _geocodingMock.Verify(g => g.Search(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Never);

            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, "DropOffAddress", "hardcoded dropoff"))), Times.Exactly(2));
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyContains(o, "StaticMapUri", "?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C9.123,6.124"))), Times.Exactly(2));
        }

        [Test]
        public void when_sending_receipt_with_dropoff_and_vehicle_position_should_geocode_vehicle_position()
        {
            var orderId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleLatitude = 45,
                    VehicleLongitude = -73,
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

            // verify that geocoding is called
            _geocodingMock.Verify(g => g.Search(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Once);

            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, "DropOffAddress", "full dropoff"))), Times.Exactly(2));
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyContains(o, "StaticMapUri", "?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C45,-73"))), Times.Exactly(2));
        }
    }
}