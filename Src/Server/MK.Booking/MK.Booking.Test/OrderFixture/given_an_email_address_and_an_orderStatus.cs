using System;
using System.Collections.Generic;
using System.Globalization;
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
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
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
                new ConfigurationDao(() => new ConfigurationDbContext(DbName)),
                new OrderDao(() => new BookingDbContext(DbName)),
                new AccountDao(() => new BookingDbContext(DbName)), 
                new StaticMap(),
                null,
                _geocodingMock.Object,
                new Logger());
            notificationService.SetBaseUrl(new Uri("http://www.example.net"));

            Sut.Setup(new EmailCommandHandler(notificationService));

            _geocodingMock
                .Setup(x => x.SearchAsync(45, -73, It.IsAny<string>(), It.IsAny<GeoResult>(), false))
                .ReturnsAsync(new [] {new Address { FullAddress = "full dropoff" }});

            TemplateServiceMock
                .Setup(x => x.InlineCss(It.IsAny<string>()))
                .Returns(string.Empty);
        }

        [Test]
        public void when_sending_receipt_template_should_have_good_values()
        {
            var orderId = Guid.NewGuid();

            var pickupDate = DateTime.Now;

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new OrderStatusDetail
                {
                    OrderId = orderId,
                    PickupDate = pickupDate
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
                ClientLanguageCode = "fr",
                BookingFees = 5,
                AmountSavedByPromotion = 2,
                IBSOrderId = 123,
                DriverInfos = new DriverInfos
                {
                    DriverId = "1",
                    FirstName = "Cosmo",
                    LastName = "Kramer",
                    MobilePhone = "5551231234",
                    VehicleColor = "Red",
                    VehicleMake = "Porsche",
                    VehicleModel = "Carrera",
                    VehicleRegistration = "GIDDYUP",
                    VehicleType = "Sport"
                },
                Extra = 1,
                Fare = 2,
                PromoCode = "promo",
                PromoDiscountType = PromoDiscountType.Cash,
                PromoDiscountValue = 2,
                PickupDate = DateTime.Now,
                Surcharge = 3,
                Tax = 1,
                Tip = 2,
                Toll = 4,
                VehicleNumber = "123",
                CmtRideLinqFields = new SendReceipt.CmtRideLinqReceiptFields()
                {
                    TipIncentive = 5
                },
            });

            var dateFormat = CultureInfo.GetCultureInfo("fr");

            AssertTemplateValueEquals("DropOffAddress", "-");
            AssertTemplateValueEquals("StaticMapUri", string.Empty);
            AssertTemplateValueEquals("ibsOrderId", "123");
            AssertTemplateValueEquals("HasDriverInfo", "True");
            AssertTemplateValueEquals("HasDriverId", "True");
            AssertTemplateValueEquals("VehicleNumber", "123");
            AssertTemplateValueEquals("DriverId", "1");
            AssertTemplateValueEquals("PickupDate", pickupDate.ToString("D", dateFormat));
            AssertTemplateValueEquals("PickupTime", pickupDate.ToString("t", dateFormat));
            AssertTemplateValueEquals("DropOffDate", pickupDate.ToString("D", dateFormat));
            AssertTemplateValueEquals("DropOffTime", string.Empty);
            AssertTemplateValueEquals("ShowDropOffTime", "False");
            AssertTemplateValueEquals("ShowUTCWarning", "True");
            AssertTemplateValueEquals("Fare", "$2.00");
            AssertTemplateValueEquals("Toll", "$4.00");
            AssertTemplateValueEquals("Surcharge", "$3.00");
            AssertTemplateValueEquals("BookingFees", "$5.00");
            AssertTemplateValueEquals("Extra", "$1.00");
            AssertTemplateValueEquals("Tip", "$2.00");
            AssertTemplateValueEquals("TotalFare", "$21.00");
            AssertTemplateValueEquals("Tax", "$1.00");
            AssertTemplateValueEquals("ShowTax", "True");
            AssertTemplateValueEquals("ShowTollTotal", "True");
            AssertTemplateValueEquals("ShowSurcharge", "True");
            AssertTemplateValueEquals("ShowBookingFees", "True");
            AssertTemplateValueEquals("vatIsEnabled", "False");
            AssertTemplateValueEquals("HasPaymentInfo", "False");
            AssertTemplateValueEquals("PaymentAmount", string.Empty);
            AssertTemplateValueEquals("PaymentMethod", string.Empty);
            AssertTemplateValueEquals("ShowFareAndPaymentDetails", "True");
            AssertTemplateValueEquals("PaymentTransactionId", string.Empty);
            AssertTemplateValueEquals("PaymentAuthorizationCode", string.Empty);
            AssertTemplateValueEquals("ShowPaymentAuthorizationCode", "False");
            AssertTemplateValueEquals("PickupAddress", "5250, rue Ferrier, Montreal, H1P 4L4");
            AssertTemplateValueEquals("ShowStaticMap", "False");
            AssertTemplateValueEquals("PromotionWasUsed", "True");
            AssertTemplateValueEquals("promoCode", "promo");
            AssertTemplateValueEquals("AmountSavedByPromotion", "$2.00");
            AssertTemplateValueEquals("TipIncentive", "$5.00");
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

                var points = GetPathPoints(orderId);
                foreach (var point in points)
                {
                    context.Save(point);
                }
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
            _geocodingMock.Verify(g => g.SearchAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Once);

            // verify templateData (2 times for subject + body)
            AssertTemplateValueEquals("DropOffAddress", "full dropoff");
            AssertTemplateValueContains("StaticMapUri", "?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C45,-73");

            // this is not the full encoded path since there's a problem with unit test ran on server (works locally but not on server)
            AssertTemplateValueContains("StaticMapUri", "&path=enc:ukutGhbq%60MgAoBt@qC??fBoBvB%7BDoJyNkBuHeEmB%5CmPiMcKeNiOyEp@??qEj@sJNJ%7DEoAiCkEyC%7DHmAaD%7DEyHiCoClLeKs@yH_d@mJaa@yT%7DLq");
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
            _geocodingMock.Verify(g => g.SearchAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Never);

            // verify templateData (2 times for subject + body)
            AssertTemplateValueEquals("DropOffAddress", "-");
            AssertTemplateValueEquals("StaticMapUri", string.Empty);
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
            _geocodingMock.Verify(g => g.SearchAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Never);

            // verify templateData (2 times for subject + body)
            AssertTemplateValueEquals("DropOffAddress", "hardcoded dropoff");
            AssertTemplateValueContains("StaticMapUri", "?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C9.123,6.124");
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
            _geocodingMock.Verify(g => g.SearchAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<GeoResult>(), false), Times.Once);

            // verify templateData (2 times for subject + body)
            AssertTemplateValueEquals("DropOffAddress", "full dropoff");
            AssertTemplateValueContains("StaticMapUri", "?markers=color:0x1EC022%7Csize:medium%7C1.23456,7.890123&markers=color:0xFF0000%7Csize:medium%7C45,-73");
        }

        private void AssertTemplateValueEquals(string key, string expectedValue)
        {
            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyEquals(o, key, expectedValue))), Times.Exactly(2));
        }

        private void AssertTemplateValueContains(string key, string expectedValue)
        {
            // verify templateData (2 times for subject + body)
            TemplateServiceMock.Verify(x => x.Render(It.IsAny<string>(), It.Is<object>(o => ObjectPropertyContains(o, key, expectedValue))), Times.Exactly(2));
        }

        private List<OrderVehiclePositionDetail> GetPathPoints(Guid orderId)
        {
            return new List<OrderVehiclePositionDetail>
            {
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 1), Latitude = 45.49834601776998, Longitude = -73.65684986114502 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 2), Latitude = 45.49870698436449, Longitude = -73.65629196166992 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 3), Latitude = 45.49843625963554, Longitude = -73.65556240081787 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 4), Latitude = 45.49843625963554, Longitude = -73.65556240081787 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 5), Latitude = 45.49792488715174, Longitude = -73.65500450134277 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 6), Latitude = 45.4973232665191, Longitude = -73.65406036376953 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 7), Latitude = 45.49915818935351, Longitude = -73.65152835845947 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 8), Latitude = 45.49969963056761, Longitude = -73.6499834060669 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 9), Latitude = 45.50069225927091, Longitude = -73.6494255065918 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 10), Latitude = 45.500541862107404, Longitude = -73.64663600921631 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 11), Latitude = 45.50282785563674, Longitude = -73.64470481872559 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 12), Latitude = 45.505264141298525, Longitude = -73.64208698272705 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 13), Latitude = 45.506346901083425, Longitude = -73.64234447479248 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 14), Latitude = 45.506346901083425, Longitude = -73.64234447479248 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 15), Latitude = 45.507399564241155, Longitude = -73.64255905151367 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 16), Latitude = 45.50926423351361, Longitude = -73.64264488220215 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 17), Latitude = 45.50920408385609, Longitude = -73.64152908325195 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 18), Latitude = 45.50959505548101, Longitude = -73.64084243774414 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 19), Latitude = 45.51061758381424, Longitude = -73.64006996154785 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 20), Latitude = 45.51221148799124, Longitude = -73.6396837234497 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 21), Latitude = 45.51302345955653, Longitude = -73.63856792449951 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 22), Latitude = 45.5145872236384, Longitude = -73.6378812789917 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 23), Latitude = 45.51530894625157, Longitude = -73.64002704620361 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 24), Latitude = 45.51726356518122, Longitude = -73.63976955413818 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 25), Latitude = 45.51882721144158, Longitude = -73.6338472366333 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 26), Latitude = 45.52066143340206, Longitude = -73.62839698791504 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 27), Latitude = 45.52414929707939, Longitude = -73.62616539001465 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 28), Latitude = 45.52871927390669, Longitude = -73.62333297729492 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 29), Latitude = 45.52871927390669, Longitude = -73.62333297729492 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 30), Latitude = 45.52871927390669, Longitude = -73.62333297729492 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 31), Latitude = 45.52871927390669, Longitude = -73.62333297729492 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 32), Latitude = 45.529981965194956, Longitude = -73.62238883972168 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 33), Latitude = 45.534701774459755, Longitude = -73.61835479736328 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 34), Latitude = 45.53545329988692, Longitude = -73.62015724182129 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 35), Latitude = 45.532056325059585, Longitude = -73.62316131591797 },
                new OrderVehiclePositionDetail{ OrderId = orderId, DateOfPosition = new DateTime(2014, 1, 1, 1, 1, 36), Latitude = 45.53184588624152, Longitude = -73.62243175506592 }
            };
        }
    }
}