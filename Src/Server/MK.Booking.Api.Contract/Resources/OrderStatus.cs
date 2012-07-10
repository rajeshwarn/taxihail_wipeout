namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderStatus
    {
        public OrderStatusStep Step { get; set; }
        public double? Latitude { get; set; }
        public double? Longititude { get; set; }
    }
}