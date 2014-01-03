#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class PairForRideLinqCmtPayment : ICommand
    {
        public PairForRideLinqCmtPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }
        public string Medallion { get; set; }
        public string DriverId { get; set; }
        public string PairingToken { get; set; }
        public string PairingCode { get; set; }
        public string TokenOfCardToBeUsedForPayment { get; set; }
        public double? AutoTipAmount { get; set; }
        public int? AutoTipPercentage { get; set; }
        public Guid Id { get; private set; }
    }
}