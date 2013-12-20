using System;

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt
{
    public class Trip
    {
        public Trip()
        {
            TollHistory = new Toll[0];
        }

        public string Type { get; set; }
        public int TripId { get; set; }
        public int DriverId { get; set; }
        public string PairingToken { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Total { get; set; }
        public int Fare { get; set; }
        public int FareAtAlternateRate { get; set; }
        public int Extra { get; set; }
        public int Tip { get; set; }
        public int Surcharge { get; set; }
        public int Tax { get; set; }
        public int RateAtTripStart { get; set; }
        public int RateAtTripEnd { get; set; }
        public string RateChangeTime { get; set; }
        public double Distance { get; set; }
        public int AutoTipPercentage { get; set; }
        public int AutoTipAmount { get; set; }
        public Toll[] TollHistory { get; set; }

        // not present in API TPEP 2.0
        public bool AutoCompletePayment { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }

        public class Toll
        {
            public string TollName { get; set; }
            public int TollAmount { get; set; }
        }

        public bool Compare(Trip otherTrip)
        {
            if (otherTrip.AutoTipPercentage != this.AutoTipPercentage)
                return false;
            if (otherTrip.AutoTipAmount != this.AutoTipAmount)
                return false;
            return true;
        }
    }
}