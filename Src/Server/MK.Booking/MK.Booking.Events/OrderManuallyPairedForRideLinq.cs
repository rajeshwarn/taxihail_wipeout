﻿using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderManuallyPairedForRideLinq : VersionedEvent
    {
        public Guid AccountId { get; set; }
        public string PairingCode { get; set; }
        public string PairingToken { get; set; }
        public DateTime PairingDate { get; set; }
        public string ClientLanguageCode { get; set; }
        public string UserAgent { get; set; }
        public string ClientVersion { get; set; }
        public double? Distance { get; set; }
        public double? Total { get; set; }
        public double? Fare { get; set; }
        public double? FareAtAlternateRate { get; set; }
        public double? Toll { get; set; }
        public double? Extra { get; set; }
        public double? Tip { get; set; }
        public double? Surcharge { get; set; }
        public double? Tax { get; set; }
        public double? RateAtTripStart { get; set; }
        public double? RateAtTripEnd { get; set; }
        public string RateChangeTime { get; set; }
        public string Medallion { get; set; }
    }
}
