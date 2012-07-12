using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/accounts/{AccountId}/bookingsettings", "PUT")]    
    public class BookingSettingsRequest
    {
        public string Name { get; set; }        

        public string Phone { get; set; }

        public int Passengers { get; set; }

        public int VehicleTypeId { get; set; }

        public int ChargeTypeId { get; set; }

        public int ProviderId { get; set; }

        public int NumberOfTaxi { get; set; }

        public Guid AccountId { get; set; }
    }
}