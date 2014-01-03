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
        public int? IbsOrderId { get; set; }
        public string IbsStatusId { get; set; }
        public string IbsStatusDescription { get; set; }
        public string VehicleNumber { get; set; }
        public double? VehicleLatitude { get; set; }
        public double? VehicleLongitude { get; set; }
        public bool FareAvailable { get; set; }
        public Guid OrderId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime? Eta { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Status + " " + Name;
        }
    }
}