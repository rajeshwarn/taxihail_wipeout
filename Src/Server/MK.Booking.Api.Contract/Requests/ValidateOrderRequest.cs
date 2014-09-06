#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/validate", "POST")]
    [Route("/account/orders/validate/{ForError}", "POST")]
    [Route("/account/orders/validate/{ForError}/{TestZone}", "POST")]
    public class ValidateOrderRequest : BaseDto
    {
        public ValidateOrderRequest()
        {
            PickupAddress = new Address();
            DropOffAddress = new Address();
            Payment = new PaymentSettings();
            Settings = new BookingSettings();
        }

        //For testing purpose, when the TestZone is set, it will be used instead of calling IBS
        public string TestZone { get; set; }

        public bool ForError { get; set; }

        public Guid Id { get; set; }

        public DateTime? PickupDate { get; set; }

        public string Note { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public PaymentSettings Payment { get; set; }
    }
}