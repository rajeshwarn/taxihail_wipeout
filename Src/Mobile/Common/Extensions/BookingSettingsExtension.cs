using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class BookingSettingsExtension
    {
        public static BookingSettings Copy(this BookingSettings instance)
        {
            var copy = new BookingSettings();
            copy.Name = instance.Name;            
            copy.Phone = instance.Phone;
            copy.Passengers = instance.Passengers;
            copy.VehicleTypeId = instance.VehicleTypeId;
            copy.ChargeTypeId = instance.ChargeTypeId;
            copy.ProviderId = instance.ProviderId;
            copy.NumberOfTaxi = instance.NumberOfTaxi;
            return copy;


        }
    }
}