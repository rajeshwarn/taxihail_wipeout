using NUnit.Framework;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services.Impl;

namespace TaxiMobile.Lib.Tests
{
    [TestFixture]
    public class BookingServiceFixture
    {
        private BookingService sut;

        [TestFixtureSetUp]
        public void Setup()
        {
            ServiceLocator.Current.Register<IAppSettings, AppSettings>();
            ServiceLocator.Current.Register<ILogger, SimpleLogger>();
            sut = new BookingService();
        }

        [Test]
        public void CreateOrderTest()
        {
            string error;
            var user = new AccountService().GetAccount("apcurium@apcurium.com", "password", out error);
            var order = new BookingInfoData
                            {
                                PickupLocation = new LocationData { Address = "5250, Ferrier, Montréal, QC H4P 1L4 ", Latitude = 45.497985, Longitude = -73.656979 }
                            };
            var result = sut.CreateOrder(user, order, out error);
            Assert.IsTrue(result > 0);
        }
    }
}