#region

using System;

#endregion

namespace apcurium.MK.Common.Entity
{
    public class OrderStatusDetail
    {
        public OrderStatusDetail()
        {
            DriverInfos = new DriverInfos();
        }

        public OrderStatus Status { get; set; }
        public DriverInfos DriverInfos { get; set; }
        public int? IBSOrderId { get; set; }
        public string IBSStatusId { get; set; }
        public string IBSStatusDescription { get; set; }
        public string VehicleNumber { get; set; }
        public double? VehicleLatitude { get; set; }
        public double? VehicleLongitude { get; set; }
        public bool FareAvailable { get; set; }
        public bool IsChargeAccountPaymentWithCardOnFile { get; set; }
        public bool IsPrepaid { get; set; }
        public Guid OrderId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime? Eta { get; set; }
        public string Name { get; set; }
        public string ReferenceNumber { get; set; }
        public string TerminalId { get; set; }
        public DateTime? PairingTimeOut { get; set; }
        public DateTime? UnpairingTimeOut { get; set; }
        public string PairingError { get; set; }

        public DateTime? TaxiAssignedDate { get; set; }
        
        // network stuff
        public string Market { get; set; }
        public string CompanyKey { get; set; }                  // not null if created on another ibs
        public string CompanyName { get; set; }                 // not null if created on another ibs
        public string NextDispatchCompanyName { get; set; }
        public string NextDispatchCompanyKey { get; set; }
        public bool IgnoreDispatchCompanySwitch { get; set; }
        public DateTime? NetworkPairingTimeout { get; set; }

        public bool IsManualRideLinq { get; set; }

        public string RideLinqPairingCode { get; set; }
        public long? OriginalEta { get; set; }
        public double? TipIncentive { get; set; }

        public override string ToString()
        {
            return Status + " " + Name;
        }
    }
}