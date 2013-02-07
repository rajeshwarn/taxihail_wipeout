using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/account/bookingsettings", "PUT")]
    public class BookingSettingsRequest : BaseDTO
    {
        public string Name { get; set; }

        public string FirstName { get; set; }

        public string AccountId { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; } 

        public string Phone { get; set; }

        public int? VehicleTypeId { get; set; }

        public int? ChargeTypeId { get; set; }

        public int? ProviderId { get; set; }

        public int NumberOfTaxi { get; set; }

        public int Passengers { get; set; }

        public Guid? DefaultCreditCard { get; set; }

        public double? DefaultTipAmount { get; set; }

        public double? DefaultTipPercent { get; set; } 
    }
}