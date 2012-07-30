using System;

using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddAddress : ICommand
    {
        public AddAddress()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AddressId { get; set; }
        public Guid AccountId { get; set; }
        public string FriendlyName { get; set; }
        public string FullAddress { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Apartment { get; set; }
        public string RingCode { get; set; }
        public bool IsHistoric { get; set; }

    }
}
