using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Web.Tests
{
    public class OrderFixture : BaseTest
    {
        private readonly Guid _orderId = Guid.NewGuid();
        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();
        }

        [TestFixtureTearDown]
        public new void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void CreateOrder()
        {
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var pickupDate = DateTime.Now;
            var requestDate = DateTime.Now.AddHours(1);
            var order = new CreateOrder
                            {
                                Id = _orderId,
                                AccountId = TestAccount.Id,
                                FriendlyName = "Chez François",
                                Apartment = "3939",
                                FullAddress = "1234 rue Saint-Hubert",
                                RingCode = "3131",
                                Latitude = 45.515065,
                                Longitude = -73.558064,
                                PickupDate = pickupDate,
                                RequestedDateTime = requestDate,
                            };

            var id = sut.CreateOrder(order);
            Assert.NotNull(id);
        }
    }
}
