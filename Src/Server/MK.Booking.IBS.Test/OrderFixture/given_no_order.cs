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

        [SetUp]
        public void Setup()
        {
            Sut = new WebOrder7Service {Url = "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/IWebOrder7"};

        }

        [Test]
        [Ignore("Test fails. MK is lokking into it.")]
        public void when_creating_an_order()
        {
            var order = new TBookOrder_7();

            order.ServiceProviderID = MobileKnowledgeProviderId;
            order.AccountID = 59;
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
    }
}
