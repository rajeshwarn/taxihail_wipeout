using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Data
{
    public class IBSOrderResult
    {
        public int? CreateOrderResult { get; set; }

        public OrderHailResult HailResult { get; set; }

        public bool IsHailRequest { get; set; }
    }
}
