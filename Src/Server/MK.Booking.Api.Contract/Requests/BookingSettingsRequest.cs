using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/account/{AccountId}/bookingsettings", "POST")]    
    public class BookingSettingsRequest
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string Phone { get; set; }

        public int Passengers { get; set; }

        public int VehicleTypeId { get; set; }

        public int ChargeTypeId { get; set; }

        public int ProviderId { get; set; }

        public int NumberOfTaxi { get; set; }

        public Guid AccountId { get; set; }
    }
}