using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class PairForRideLinqCmtPayment : ICommand
    {
        public PairForRideLinqCmtPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
        public string Medallion { get; set; }
        public string DriverId { get; set; }
        public string PairingToken { get; set; }
        public string PairingCode { get; set; }
        public string TokenOfCardToBeUsedForPayment { get; set; }
        public double? AutoTipAmount { get; set; }
        public int? AutoTipPercentage { get; set; }
    }
}