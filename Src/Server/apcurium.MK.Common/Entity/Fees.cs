namespace apcurium.MK.Common.Entity
{
    public class Fees
    {
        public string Market { get; set; }

        public decimal Booking { get; set; }

        public decimal Cancellation { get; set; }

        public decimal NoShow { get; set; }
    }
}