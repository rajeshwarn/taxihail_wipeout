using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateBookingSettings : ICommand
    {
        public UpdateBookingSettings()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }        
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Passengers { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? ChargeTypeId { get; set; }
        public int? ProviderId { get; set; }
        public int NumberOfTaxi { get; set; }

        public Guid? DefaultCreditCard { get; set; }
        public double? DefaultTipAmount { get; set; }
        public double? DefaultTipPercent { get; set; }

        public Guid AccountId { get; set; }
    }
}