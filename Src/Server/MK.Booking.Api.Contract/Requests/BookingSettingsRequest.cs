﻿#region

using System;
using apcurium.MK.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/bookingsettings", "PUT")]
    public class BookingSettingsRequest : BaseDto
    {
        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public CountryISOCode Country { get; set; }

        public string Phone { get; set; }

        public int? VehicleTypeId { get; set; }

        public string VehicleType { get; set; }

		public int? LuxuryVehicleTypeId { get; set; }

        public string LuxuryVehicleType { get; set;  }

		public ServiceType ServiceType { get; set; }

        public int? ChargeTypeId { get; set; }

        public int? ProviderId { get; set; }

        public int NumberOfTaxi { get; set; }

        public int Passengers { get; set; }

        public string AccountNumber { get; set; }

        public string CustomerNumber { get; set; }

        public int? DefaultTipPercent { get; set; }

        public string PayBack { get; set; }
    }
}