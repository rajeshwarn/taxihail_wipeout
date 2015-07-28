#region

using apcurium.MK.Common;
using Infrastructure.EventSourcing;
using System;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class BookingSettingsUpdated : VersionedEvent
    {
        public string Name { get; set; }

        public CountryISOCode Country { get; set; }
        
        public string Phone { get; set; }

        public int Passengers { get; set; }

        public int? ProviderId { get; set; }

        public int? VehicleTypeId { get; set; }

        public int? ChargeTypeId { get; set; }

        public int NumberOfTaxi { get; set; }

        public string AccountNumber { get; set; }

        public string CustomerNumber { get; set; }

        public int? DefaultTipPercent { get; set; }

        public string PayBack { get; set; }
    }
}