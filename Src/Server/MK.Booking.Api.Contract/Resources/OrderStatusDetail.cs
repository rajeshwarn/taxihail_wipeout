using System;
namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderStatusDetail
    {
        public OrderStatus Status { get; set; }
        public int? IBSOrderId { get; set; }                
        public string IBSStatusId { get; set; }
        public string IBSStatusDescription { get; set; }
        public double? VehicleLatitude { get; set; }
        public double? VehicleLongitude { get; set; }
        public Guid OrderId { get; set; }
    }
}