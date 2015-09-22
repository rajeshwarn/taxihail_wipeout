using apcurium.MK.Common.Entity;
using System;

namespace CMTPayment.Pair
{
    public class Trip
    {
        public Trip()
        {
            TollHistory = new TollDetail[0];
        }

        public string Type { get; set; }
        public int TripId { get; set; }
        public int DriverId { get; set; }
        public string Medallion { get; set; }
        public string PairingToken { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Total { get; set; }
        public int Fare { get; set; }
        public int FareAtAlternateRate { get; set; }
        public int Extra { get; set; }
        public int Tip { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public int Surcharge { get; set; }
        public int Tax { get; set; }
        public int RateAtTripStart { get; set; }
        public int RateAtTripEnd { get; set; }
        public string RateChangeTime { get; set; }
        public double Distance { get; set; }
        public int? AutoTipPercentage { get; set; }
        public int? AutoTipAmount { get; set; }
        public TollDetail[] TollHistory { get; set; }
        public bool AutoCompletePayment { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string LastFour { get; set; }
        public int AccessFee { get; set; }
        public int? ErrorCode { get; set; }

        public bool Compare(Trip otherTrip)
        {
            if (otherTrip.AutoTipPercentage != AutoTipPercentage)
            {
                return false;
            }

            if (otherTrip.AutoTipAmount != AutoTipAmount)
            {
                return false;
            }
                
            return true;
        }
    }
}