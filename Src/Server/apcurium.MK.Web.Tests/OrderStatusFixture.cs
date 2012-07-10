using System;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class OrderStatusFixture : BaseTest
    {
        private readonly Guid _orderId = Guid.NewGuid();
        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();
            var sut = new OrderServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var order = new CreateOrder
            {
                Id = _orderId,
                AccountId = TestAccount.Id,
                PickupApartment = "3939",
                PickupAddress = "1234 rue Saint-Hubert",
                PickupRingCode = "3131",
                PickupLatitude = 45.515065,
                PickupLongitude = -73.558064,
                PickupDate = DateTime.Now,
                DropOffAddress = "Velvet auberge st gabriel",
                DropOffLatitude = 45.50643,
                DropOffLongitude = -73.554052
            };
            sut.CreateOrder(order);
        }

        [TestFixtureTearDown]
        public new void TearDown()
        {
            base.TearDown();
        }

        [SetUp]
        public void SetupTest()
        {

        }
        [Test]
        public void Get()
        {
            var sut = new OrderStatusClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var data = sut.GetStatus(_orderId);
            Assert.AreEqual(OrderStatus.Created.ToString(),data);
        }
    }
}