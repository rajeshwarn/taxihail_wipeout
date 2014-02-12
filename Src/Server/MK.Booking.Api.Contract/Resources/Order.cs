#region

using System;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class Order : BaseDto
    {
        public Guid Id { get; set; }

        public int? IBSOrderId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Note { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public double? Fare { get; set; }

        public double? Toll { get; set; }

        public double? Tip { get; set; }

        public bool IsRated { get; set; }

        public long TransactionId { get; set; }

        public OrderStatus Status { get; set; }
    }
}