using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel;
using OrderStatus = apcurium.MK.Booking.Api.Contract.Resources.OrderStatus;

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
            resource.PickupAddress = new apcurium.MK.Booking.Api.Contract.Resources.Address { FullAddress = order.PickupAddress, Apartment = order.PickupApartment, RingCode = order.PickupRingCode, Latitude = order.PickupLatitude, Longitude = order.PickupLongitude };
            resource.DropOffAddress = new apcurium.MK.Booking.Api.Contract.Resources.Address { FullAddress = order.DropOffAddress, Latitude = order.DropOffLatitude.HasValue ? order.DropOffLatitude.Value : 0, Longitude = order.DropOffLongitude.HasValue ? order.DropOffLongitude.Value : 0 };
            resource.Settings = new BookingSettings { ChargeTypeId = order.Settings.ChargeTypeId, Name = order.Settings.Name, Phone = order.Settings.Phone, NumberOfTaxi = 1, Passengers = order.Settings.Passengers, ProviderId = order.Settings.ProviderId, VehicleTypeId = order.Settings.VehicleTypeId };
            resource.Fare = order.Fare;
            resource.Tip = order.Tip;
            resource.Toll = order.Toll;
            resource.IsCompleted = order.Status == (int) OrderStatus.Completed;
            return resource;
        }
    }
}
