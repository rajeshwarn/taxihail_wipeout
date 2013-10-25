using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class OrderStatusFixture : BaseTest
    {
        private Guid _orderId;
       
        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            

            base.TestFixtureSetup();

            var authResponse = new AuthServiceClient(BaseUrl, null, "Test").Authenticate(TestAccount.Email, TestAccountPassword);

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
                }
            };

            sut.CreateOrder(order);
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {            
            base.TestFixtureTearDown();
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void create_and_get_a_valid_order()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var data = sut.GetOrderStatus( _orderId);
         
            Assert.AreEqual(OrderStatus.Created, data.Status);
            Assert.AreEqual("Joe Smith", data.Name);
        }



        [Test]
        public void can_not_access_order_status_another_account()
        {
            CreateAndAuthenticateTestAccount();

            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");

            Assert.Throws<WebServiceException>(() => sut.GetOrderStatus(_orderId));
        }

        [Test]
        public void get_active_orders_status()
        {
            var sut = new OrderServiceClient(BaseUrl, SessionId, "Test");
            var data = sut.GetActiveOrdersStatus();


            Assert.AreEqual(true, data.Any());
            Assert.AreEqual(true, data.Any(x => x.OrderId == _orderId));
            //Assert.AreEqual(null, data.First(x => x.OrderId == _orderId).IBSStatusId);
            Assert.AreEqual(OrderStatus.Created, data.First(x => x.OrderId == _orderId).Status);
            
        }
    }
}