﻿#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class OrderCreated : VersionedEvent
    {
        public OrderCreated()
        {
            Settings = new BookingSettings();
        }

        public Guid AccountId { get; set; }

        public int IBSOrderId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public double? EstimatedFare { get; set; }

        public string UserAgent { get; set; }

        public string ClientLanguageCode { get; set; }

        public double? UserLatitude { get; set; }

        public double? UserLongitude { get; set; }
        
    }
}