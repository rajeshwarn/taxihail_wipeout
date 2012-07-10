namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderStatus
    {
        public int IBSOrderid { get; set; }
        public OrderStatusStep Step { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}