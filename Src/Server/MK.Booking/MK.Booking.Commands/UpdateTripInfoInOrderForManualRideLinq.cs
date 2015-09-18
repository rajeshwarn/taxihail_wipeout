using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateTripInfoInOrderForManualRideLinq : ICommand
    {
        public UpdateTripInfoInOrderForManualRideLinq()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public double? Distance { get; set; }

        public double? Total { get; set; }

        public double? Fare { get; set; }

        public double? FareAtAlternateRate { get; set; }

        public double? TollTotal { get; set; }

        public double? Extra { get; set; }

        public double? Tip { get; set; }

        public double? Surcharge { get; set; }

        public double? Tax { get; set; }

        public int? RateAtTripStart { get; set; }

        public int? RateAtTripEnd { get; set; }

        public string RateChangeTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string PairingToken { get; set; }

        public int TripId { get; set; }

        public int DriverId { get; set; }

        public double? AccessFee { get; set; }

        public string LastFour { get; set; }

        public double? LastLatitudeOfVehicle { get; set; }
        public double? LastLongitudeOfVehicle { get; set; }
        public TollDetail[] Tolls { get; set; }
    }
}