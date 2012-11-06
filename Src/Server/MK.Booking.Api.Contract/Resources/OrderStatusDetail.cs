using System;
namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderStatusDetail : BaseDTO
    {
        public Resources.OrderStatus Status { get; set; }
        public int? IBSOrderId { get; set; }                
        public string IBSStatusId { get; set; }
        public string IBSStatusDescription { get; set; }
        public string VehicleNumber { get; set; }
        public double? VehicleLatitude { get; set; }
        public double? VehicleLongitude { get; set; }
        public Guid OrderId { get; set; }
    }
}