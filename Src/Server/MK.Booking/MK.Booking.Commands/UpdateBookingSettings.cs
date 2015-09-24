#region

using System;
using Infrastructure.Messaging;
using apcurium.MK.Common;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class UpdateBookingSettings : ICommand
    {
        public UpdateBookingSettings()
        {
            Id = Guid.NewGuid();
        }

		public string Email { get; set; }

        public string Name { get; set; }

        public CountryISOCode Country { get; set; }

        public string Phone { get; set; }

        public int Passengers { get; set; }

        public int? VehicleTypeId { get; set; }

        public int? ChargeTypeId { get; set; }

        public int? ProviderId { get; set; }

        public int NumberOfTaxi { get; set; }

        public string AccountNumber { get; set; }

        public string CustomerNumber { get; set; }

        public int? DefaultTipPercent { get; set; }

        public string PayBack { get; set; }

        public Guid AccountId { get; set; }

        public Guid Id { get; set; }
    }
}