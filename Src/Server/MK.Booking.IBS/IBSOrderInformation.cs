#region

using System;
using System.Linq;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.IBS
{
    /// <summary>
    ///     class with all the data about the order when requesting  status of a list of order
    /// </summary>
    public class IBSOrderInformation
    {
        public int IBSOrderId { get; set; }
        public string Status { get; set; }
        public double? VehicleLatitude { get; set; }
        public double? VehicleLongitude { get; set; }
        public double Toll { get; set; }
        public double Fare { get; set; }
        public double Tip { get; set; }
        public double VAT { get; set; }
        public string VehicleNumber { get; set; }
        /*DriversInfos*/
        public string VehicleType;
        public string VehicleMake;
        public string VehicleModel;
        public string VehicleColor;
        public string VehicleRegistration;
        public string FirstName;
        public string LastName;
        public string MobilePhone;

        public string ReferenceNumber { get; set; }
        public string TerminalId { get; set; }
        public string DriverId { get; set; }

        public DateTime? Eta { get; set; }

        public override string ToString()
        {
            return Status + " " + FirstName;
        }

        public IBSOrderInformation()
        {

        }

        public IBSOrderInformation(TOrderStatus_2 orderInfoFromIBS)
        {
            Status = orderInfoFromIBS.OrderStatus.ToString();

            IBSOrderId = orderInfoFromIBS.OrderID;

            VehicleNumber = orderInfoFromIBS.VehicleNumber == null ? VehicleNumber : orderInfoFromIBS.VehicleNumber.Trim(); ;
            MobilePhone = orderInfoFromIBS.DriverMobilePhone.GetValue(MobilePhone);
            FirstName = orderInfoFromIBS.DriverFirstName.GetValue(FirstName);
            LastName = orderInfoFromIBS.DriverLastName.GetValue(LastName);
            VehicleColor = orderInfoFromIBS.VehicleColor.GetValue(VehicleColor);
            VehicleMake = orderInfoFromIBS.VehicleMake.GetValue(VehicleMake);
            VehicleModel = orderInfoFromIBS.VehicleModel.GetValue(VehicleModel);
            VehicleRegistration = orderInfoFromIBS.VehicleRegistration.GetValue(VehicleRegistration);

            VehicleLatitude = orderInfoFromIBS.VehicleCoordinateLat != 0 ? orderInfoFromIBS.VehicleCoordinateLat : VehicleLatitude;
            VehicleLongitude = orderInfoFromIBS.VehicleCoordinateLong != 0 ? orderInfoFromIBS.VehicleCoordinateLong : VehicleLongitude;

            DriverId = orderInfoFromIBS.CallNumber.GetValue(DriverId);

            ReferenceNumber = orderInfoFromIBS.ReferenceNumber.GetValue(ReferenceNumber);
            TerminalId = orderInfoFromIBS.TerminalId.GetValue(TerminalId);

            Fare = orderInfoFromIBS.Fare;
            Tip = orderInfoFromIBS.Tips;
            Toll = orderInfoFromIBS.Tolls;
            VAT = orderInfoFromIBS.VAT;

            Eta = orderInfoFromIBS.ETATime.ToDateTime();
        }

        public bool IsAssigned
        {
            get { return Status.SoftEqual(VehicleStatuses.Common.Assigned); }
        }

        public bool IsComplete
        {
            get { return VehicleStatuses.DoneStatuses.Any(s => s.SoftEqual(Status)); }
        }

        public bool IsCanceled
        {
            get { return VehicleStatuses.CancelStatuses.Any(s => s.SoftEqual(Status)); }
        }

        public bool IsTimedOut
        {
            get { return Status.SoftEqual(VehicleStatuses.Common.Timeout); }
        }

        public bool IsLoaded
        {
            get { return Status.SoftEqual(VehicleStatuses.Common.Loaded); }
        }

        public bool IsMeterOffNotPaid
        {
            get { return Status.SoftEqual(VehicleStatuses.Common.MeterOffNotPayed); }
        }
    }
}