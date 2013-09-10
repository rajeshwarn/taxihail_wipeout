using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using OrderStatus = apcurium.MK.Common.Entity.OrderStatus;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderMapper
    {
        public Order ToResource(OrderDetail order)
        {
            var resource = new Order();
            resource.Id = order.Id;
            resource.PickupDate = order.PickupDate;
            resource.CreatedDate = order.CreatedDate;
            resource.IBSOrderId = order.IBSOrderId;
            resource.PickupAddress = order.PickupAddress;
            resource.DropOffAddress = order.DropOffAddress;
            resource.Settings = new BookingSettings { ChargeTypeId = order.Settings.ChargeTypeId, Name = order.Settings.Name, Phone = order.Settings.Phone, NumberOfTaxi = 1, Passengers = order.Settings.Passengers, ProviderId = order.Settings.ProviderId, VehicleTypeId = order.Settings.VehicleTypeId, LargeBags = order.Settings.LargeBags };
            resource.Fare = order.Fare;
            resource.Tip = order.Tip;
            resource.Toll = order.Toll;
            resource.Status = (OrderStatus)order.Status;
            resource.IsRated = order.IsRated;
            resource.TransactionId = order.TransactionId;

            return resource;
        }
    }
}
