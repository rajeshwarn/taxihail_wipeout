using NUnit.Framework;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Services.Impl;

namespace TaxiMobile.Lib.Tests
{
    [TestFixture]
    public class BookingServiceFixture : BaseTest
    {
        private BookingService sut;

        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();
            sut = new BookingService();
        }

        [Test]
        public void CreateOrderTest()
        {
            string error;
            var user = new AccountService().GetAccount("apcurium@apcurium.com", "password", out error);
            //var order = new BookingInfoData
            //                {
            //                    PickupLocation = new LocationData { Address = "5250, Ferrier, Montréal, QC H4P 1L4 ", Latitude = 45.497985, Longitude = -73.656979 },
            //                    DestinationLocation = new LocationData { Address = "5661, Chateaubriand, Montréal, QC H2S 0B6 " }
            //                };
            //var result = sut.CreateOrder(user, order, out error);
            //Assert.IsTrue(result > 0);
            var status = sut.GetOrderStatus(user, 170979);
            Assert.AreEqual(OrderStatus.WsStatus.wosSCHED, status.Status);
        }

        [Test]
        public void GetOrderStatus()
        {
            string error;
            var user = new AccountService().GetAccount("apcurium@apcurium.com", "password", out error);
           
            var result = sut.GetOrderStatus(user, 170971);
            Assert.AreEqual(OrderStatus.WsStatus.wosSCHED , result.Status);
        }
    }
}