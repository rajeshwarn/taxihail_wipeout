#region

using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class OrderStatusChanged : VersionedEvent
    {
        public OrderStatusDetail Status { get; set; }

        public double? Fare { get; set; }

        public double? Toll { get; set; }

        public double? Tip { get; set; }

        public double? Tax { get; set; }

        public double? Surcharge { get; set; }

        public double? Extra { get; set; }

        public bool IsCompleted { get; set; }

        public string PreviousIBSStatusId { get; set; }

        public ServiceType ServiceType { get; set; }
    }
}