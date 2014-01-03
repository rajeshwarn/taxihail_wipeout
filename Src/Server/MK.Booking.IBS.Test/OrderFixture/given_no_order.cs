#region

using System;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;

#endregion

namespace MK.Booking.IBS.Test.OrderFixture
{
    [TestFixture]
    public class given_no_order
    {
        [SetUp]
        public void Setup()
        {
            Sut = new WebOrder7Service {Url = "http://apcuriumibs:6928/XDS_IASPI.DLL/soap/IWebOrder7"};
            _accountId = CreateIBSAccount();
        }

        private const int TheChauffeurGroupProviderId = 18;

        protected WebOrder7Service Sut;
        private int _accountId;

        private int CreateIBSAccount()
        {
            var service = new WebAccount3Service
            {
                Url = "http://apcuriumibs:6928/XDS_IASPI.DLL/soap/IWebAccount3"
            };

            var account = new TBookAccount3
            {
                WEBID = Guid.NewGuid().ToString().Substring(0, 5),
                Address = new TWEBAddress(),
                Email2 = "vincent.costel@apcurium.com",
                Title = "",
                FirstName = "Apcurium",
                LastName = "Test",
                Phone = "5141234569",
                MobilePhone = "5141234569",
                WEBPassword = "123456",
                AccType = TAccountType.actWebAccount,
            };

            var ibsAcccountId = service.SaveAccount3("taxi", "test", account);
            Trace.WriteLine("IBS account created: " + ibsAcccountId);

            return ibsAcccountId;
        }

        [Test]
        public void when_creating_an_order()
        {
            var order = new TBookOrder_7();
            order.ServiceProviderID = TheChauffeurGroupProviderId;
            order.AccountID = _accountId;
            var pickupDateTime = DateTime.Now.AddMinutes(5);
            order.PickupDate = new TWEBTimeStamp
            {
                Year = pickupDateTime.Year,
                Month = pickupDateTime.Month,
                Day = pickupDateTime.Day
            };
            order.PickupTime = new TWEBTimeStamp
            {
                Hour = pickupDateTime.Hour,
                Minute = pickupDateTime.Minute,
                Second = 0,
                Fractions = 0
            };
            order.PickupAddress = new TWEBAddress
            {
                StreetPlace = "5252, rue ferrier, Montreal, H4P2H5",
                Latitude = 45.498068,
                Longitude = -73.656916
            };
            order.DropoffAddress = new TWEBAddress {StreetPlace = " ", Longitude = 0.00, Latitude = 0.00};
            order.Note = "This is a test";
            order.Phone = "5146543024";
            order.ContactPhone = "5146543024";
            order.OrderDate = order.PickupDate;
            order.VehicleTypeID = 1;
            order.OrderStatus = TWEBOrderStatusValue.wosPost;


            var orderService = new WebOrder7Service {Url = "http://apcuriumibs:6928/XDS_IASPI.DLL/soap/IWebOrder7"};
            var orderId = orderService.SaveBookOrder_7("taxi", "test", order);

            Assert.Greater(orderId, 0);
            Trace.TraceInformation(orderId.ToString(CultureInfo.InvariantCulture));
        }
    }
}