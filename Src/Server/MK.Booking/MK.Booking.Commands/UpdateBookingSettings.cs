#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdateBookingSettings : ICommand
    {
        public UpdateBookingSettings()
        {
            Id = Guid.NewGuid();
        }

        public string Name { get; set; }
        public string Phone { get; set; }
        public int Passengers { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? ChargeTypeId { get; set; }
        public int? ProviderId { get; set; }
        public int NumberOfTaxi { get; set; }
        public string AccountNumber { get; set; }

        public Guid? DefaultCreditCard { get; set; }
        public int? DefaultTipPercent { get; set; }

        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
    }
}