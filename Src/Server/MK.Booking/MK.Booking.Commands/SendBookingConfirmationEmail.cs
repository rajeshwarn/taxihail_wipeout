#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class SendBookingConfirmationEmail : ICommand
    {
        public SendBookingConfirmationEmail()
        {
            Id = Guid.NewGuid();
        }

        public string EmailAddress { get; set; }
        public int IBSOrderId { get; set; }
        public DateTime PickupDate { get; set; }
        public Address PickupAddress { get; set; }
        public Address DropOffAddress { get; set; }
        public InternalBookingSettings Settings { get; set; }
        public string ClientLanguageCode { get; set; }
        public string Note { get; set; }
        public Guid Id { get; set; }

        public class InternalBookingSettings
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public int Passengers { get; set; }
            public string VehicleType { get; set; }
            public string ChargeType { get; set; }
            public int LargeBags { get; set; }
        }
    }
}