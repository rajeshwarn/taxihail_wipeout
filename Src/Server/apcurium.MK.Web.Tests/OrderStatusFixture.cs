using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class OrderStatusFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        private Guid _orderId;

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();

            var authResponseTask = new AuthServiceClient(BaseUrl, null, "Test").Authenticate(TestAccount.Email, TestAccountPassword);
            authResponseTask.Wait();
            var authResponse = authResponseTask.Result;

            _orderId = Guid.NewGuid();
            var sut = new OrderServiceClient(BaseUrl, authResponse.SessionId, "Test");
            var order = new CreateOrder
            {
                Id = _orderId,
                PickupAddress = TestAddresses.GetAddress1(),
                DropOffAddress = TestAddresses.GetAddress2(),
                PickupDate = DateTime.Now,
                Settings = new BookingSettings
                {
                    ChargeTypeId = 99,
                    VehicleTypeId = 1,
                    ProviderId = Provider.MobileKnowledgeProviderId,
                    Phone = "514-555-1212",
                    Passengers = 6,
                    NumberOfTaxi = 1,
                    Name = "Joe Smith"
                },
                Estimate = new CreateOrder.RideEstimate
                {
                    Distance = 3,
                    Price = 10
                },
                ClientLanguageCode = "fr"
            };

            sut.CreateOrder(order).Wait();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [Test]
        public async void can_not_access_order_status_another_account()
        {
            await CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            Assert.Throws<WebServiceException>(async () => await sut.GetOrderStatus(_orderId));
        }

        [Test]
        public async void create_and_get_a_valid_order()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var data = await sut.GetOrderStatus(_orderId);

            Assert.AreEqual(OrderStatus.Created, data.Status);
            Assert.AreEqual("Joe Smith", data.Name);
        }

        [Test]
        public async void get_active_orders_status()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var data = await sut.GetActiveOrdersStatus();
            Assert.AreEqual(true, data.Any());
            Assert.AreEqual(true, data.Any(x => x.OrderId == _orderId));
            Assert.AreEqual(OrderStatus.Created, data.First(x => x.OrderId == _orderId).Status);
        }
    }
}