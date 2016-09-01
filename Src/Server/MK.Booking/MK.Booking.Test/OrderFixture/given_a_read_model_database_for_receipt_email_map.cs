using System;
using System.Web.Routing;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using MK.Common.Configuration;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    public class given_a_read_model_database_for_receipt_email_map : given_a_read_model_database
    {
        protected TestServerSettings ConfigurationManager;
        protected Mock<IEmailSender> EmailSenderMock;
        protected Mock<ITemplateService> TemplateServiceMock;
        protected EventSourcingTestHelper<Order> Sut;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = Guid.NewGuid(),
                    Email = "test2@example.net",
                    CreationDate = DateTime.Now
                });
            }

            using (var context = new ConfigurationDbContext(DbName))
            {
                context.Save(new NotificationSettings
                {
                    Id = AppConstants.CompanyId,
                    Enabled = true,
                    BookingConfirmationEmail = true,
                    ConfirmPairingPush = true,
                    DriverAssignedPush = true,
                    PaymentConfirmationPush = true,
                    NearbyTaxiPush = true,
                    ReceiptEmail = true,
                    PromotionUnlockedEmail = true,
                    VehicleAtPickupPush = true,
                    UnpairingReminderPush = true,
                    DriverBailedPush = true,
                    NoShowPush = true
                });
            }
        }

        [SetUp]
        public void Setup()
        {
            Sut = new EventSourcingTestHelper<Order>();

            EmailSenderMock = new Mock<IEmailSender>();
            ConfigurationManager = new TestServerSettings();
            TemplateServiceMock = new Mock<ITemplateService>();

            TemplateServiceMock.Setup(x => x.Render(It.IsAny<string>(), It.IsAny<object>())).Returns("");
            TemplateServiceMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>())).Returns("");
        }

        protected bool ObjectPropertyEquals(object o, string propertyName, string expectedValue)
        {
            return new RouteValueDictionary(o)[propertyName].ToString() == expectedValue;
        }

        protected bool ObjectPropertyContains(object o, string propertyName, string expectedValue)
        {
            return new RouteValueDictionary(o)[propertyName].ToString().Contains(expectedValue);
        }
    }
}