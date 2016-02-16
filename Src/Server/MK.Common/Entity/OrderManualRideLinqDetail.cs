using System;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Common.Entity
{
    public class OrderManualRideLinqDetail 
    {
        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }

        // This is the code displayed on the taxi rig for the user to type
        public string PairingCode { get; set; }

        // This is the token to use to Get or Delete info.
        public string PairingToken { get; set; }

        /// <summary>
        /// this is set as DateTime.Now from the server when pairing occurs
        /// </summary>
        public DateTime PairingDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsCancelled { get; set; }

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

	    public string DeviceName { get; set; }

        public int TripId { get; set; }

        public int DriverId { get; set; }

        public double? AccessFee { get; set; }

        public string LastFour { get; set; }

        public string PairingError { get; set; }

        public bool IsWaitingForPayment { get; set; }
    }
}
