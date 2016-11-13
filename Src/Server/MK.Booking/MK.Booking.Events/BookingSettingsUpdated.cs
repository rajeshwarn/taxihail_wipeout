﻿#region

using apcurium.MK.Common;
using Infrastructure.EventSourcing;
using System;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class BookingSettingsUpdated : VersionedEvent
    {
        public string Name { get; set; }

		public string Email { get; set; }

        public CountryISOCode Country { get; set; }
        
        public string Phone { get; set; }

        public int Passengers { get; set; }

        public int? ProviderId { get; set; }

        public int? VehicleTypeId { get; set; }

        public string VehicleType { get; set;  }

        public int? LuxuryVehicleTypeId { get; set; }

        public string LuxuryVehicleType { get; set; }

        public ServiceType ServiceType { get; set; }

        public int? ChargeTypeId { get; set; }

        public int NumberOfTaxi { get; set; }

        public string AccountNumber { get; set; }

        public string CustomerNumber { get; set; }

        public int? DefaultTipPercent { get; set; }

        public string PayBack { get; set; }
    }
}