using System;

namespace apcurium.MK.Common.Entity
{
    public class OrderPairingDetail
    {
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