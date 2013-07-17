using System;
using System.Diagnostics;
using NUnit.Framework;

namespace MK.Booking.IBS.Test.OrderFixture
{
    [TestFixture]
    public class given_no_order
    {
        private const int TheChauffeurGroupProviderId = 17;
        private const int MobileKnowledgeProviderId = 18;

        protected WebOrder7Service Sut;
        private int _accountId;

        [SetUp]
        public void Setup()
        {
            Sut = new WebOrder7Service {Url = "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/IWebOrder7"};
            _accountId = CreateIBSAccount();


        }

        [Test]
        [Ignore("MK is working on a fix for this")]
        public void when_creating_an_order()
        {
            var order = new TBookOrder_7();
            order.ServiceProviderID = MobileKnowledgeProviderId;
            order.AccountID = _accountId;
            var pickupDateTime = DateTime.Now;
            order.PickupDate = new TWEBTimeStamp { Year = pickupDateTime.Year, Month = pickupDateTime.Month, Day = pickupDateTime.Day };
            order.PickupTime = new TWEBTimeStamp { Hour = pickupDateTime.Hour, Minute = pickupDateTime.Minute, Second = 0, Fractions = 0 };
            order.PickupAddress = new TWEBAddress { StreetPlace = "LHR", Longitude = -0.438100, Latitude = 51.481100 };
            order.DropoffAddress = new TWEBAddress { StreetPlace = " ", Longitude = 0.00, Latitude = 0.00 };
            order.Note = "This is a test";
            order.Phone = "9056667777";
            order.ContactPhone = "9056667777";
            order.OrderDate = order.PickupDate;
            order.VehicleTypeID = 7;
            order.OrderStatus = TWEBOrderStatusValue.wosPost;


            var orderService = new WebOrder7Service { Url = "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/IWebOrder7" };
            var orderId = orderService.SaveBookOrder_4("taxi", "test", order);

            Assert.Greater(orderId, 0);
            Trace.TraceInformation(orderId.ToString());
        }

        private int CreateIBSAccount()
        {
            var service = new WebAccount3Service
            {
                Url = "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/IWebAccount3"
            };

            var account = new TBookAccount3
            {
                WEBID = Guid.NewGuid().ToString().Substring(0, 5),
                Address = new TWEBAddress() { },
                Email2 = "vincent.costel@apcurium.com",
                Title = "",
                FirstName = "Apcurium",
                LastName = "Test",
                Phone = "5141234569",
                MobilePhone = "5141234569",
                WEBPassword = "123456"
            };

            var ibsAcccountId = service.SaveAccount3("taxi", "test", account);
            Trace.WriteLine("IBS account created: " + ibsAcccountId);

            return ibsAcccountId;
        }
    }
}
