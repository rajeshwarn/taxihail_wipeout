using System;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Common.Entity
{
    public class OrderManualRideLinqDetail : RideLinqInfoResponse
    {

        public Guid AccountId { get; set; }

        // This is the code displayed on the taxi rig for the user to type

        // This is the token to use to Get or Delete info.
        public string PairingToken { get; set; }

        public DateTime PairingDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsCancelled { get; set; }

        public double? FareAtAlternateRate { get; set; }

        public double? RateAtTripStart { get; set; }

        public double? RateAtTripEnd { get; set; }

        public string RateChangeTime { get; set; }

        public int TripId { get; set; }

        public double? AccessFee { get; set; }

        public string LastFour { get; set; }
    }
}
