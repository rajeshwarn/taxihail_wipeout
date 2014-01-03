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
    public class IbsOrderInformation
    {
        public string FirstName;
        public string LastName;
        public string MobilePhone;
        public string VehicleColor;
        public string VehicleMake;
        public string VehicleModel;
        public string VehicleRegistration;
        public string VehicleType;

        public IbsOrderInformation()
        {
        }

        public IbsOrderInformation(TOrderStatus orderInfoFromIbs)
        {
            Status = orderInfoFromIbs.OrderStatus.ToString();

            IbsOrderId = orderInfoFromIbs.OrderID;

            VehicleNumber = orderInfoFromIbs.VehicleNumber == null
                ? VehicleNumber
                : orderInfoFromIbs.VehicleNumber.Trim();
            
            MobilePhone = orderInfoFromIbs.DriverMobilePhone.GetValue(MobilePhone);
            FirstName = orderInfoFromIbs.DriverFirstName.GetValue(FirstName);
            LastName = orderInfoFromIbs.DriverLastName.GetValue(LastName);
            VehicleColor = orderInfoFromIbs.VehicleColor.GetValue(VehicleColor);
            VehicleMake = orderInfoFromIbs.VehicleMake.GetValue(VehicleMake);
            VehicleModel = orderInfoFromIbs.VehicleModel.GetValue(VehicleModel);
            VehicleRegistration = orderInfoFromIbs.VehicleRegistration.GetValue(VehicleRegistration);

// ReSharper disable CompareOfFloatsByEqualityOperator
            VehicleLatitude = orderInfoFromIbs.VehicleCoordinateLat != 0

                ? orderInfoFromIbs.VehicleCoordinateLat
                : VehicleLatitude;
            VehicleLongitude = orderInfoFromIbs.VehicleCoordinateLong != 0
                ? orderInfoFromIbs.VehicleCoordinateLong
                : VehicleLongitude;
// ReSharper restore CompareOfFloatsByEqualityOperator
            Fare = orderInfoFromIbs.Fare;
            Tip = orderInfoFromIbs.Tips;
            Toll = orderInfoFromIbs.Tolls;
            VAT = orderInfoFromIbs.VAT;

            Eta = orderInfoFromIbs.ETATime.ToDateTime();
        }

        public int IbsOrderId { get; set; }
        public string Status { get; set; }
        public double? VehicleLatitude { get; set; }
        public double? VehicleLongitude { get; set; }
        public double Toll { get; set; }
        public double Fare { get; set; }
        public double Tip { get; set; }
        public double VAT { get; set; }
        public string VehicleNumber { get; set; }
        /*DriversInfos*/

        public DateTime? Eta { get; set; }

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

        public override string ToString()
        {
            return Status + " " + FirstName;
        }

        public void Update(OrderStatusDetail orderStatusDetail)
        {
            orderStatusDetail.IbsStatusId = Status;

            orderStatusDetail.DriverInfos.FirstName = FirstName.GetValue(orderStatusDetail.DriverInfos.FirstName);
            orderStatusDetail.DriverInfos.LastName = LastName.GetValue(orderStatusDetail.DriverInfos.LastName);
            orderStatusDetail.DriverInfos.MobilePhone = MobilePhone.GetValue(orderStatusDetail.DriverInfos.MobilePhone);
            orderStatusDetail.DriverInfos.VehicleColor =
                VehicleColor.GetValue(orderStatusDetail.DriverInfos.VehicleColor);
            orderStatusDetail.DriverInfos.VehicleMake = VehicleMake.GetValue(orderStatusDetail.DriverInfos.VehicleMake);
            orderStatusDetail.DriverInfos.VehicleModel =
                VehicleModel.GetValue(orderStatusDetail.DriverInfos.VehicleModel);
            orderStatusDetail.DriverInfos.VehicleRegistration =
                VehicleRegistration.GetValue(orderStatusDetail.DriverInfos.VehicleRegistration);
            orderStatusDetail.DriverInfos.VehicleType = VehicleType.GetValue(orderStatusDetail.DriverInfos.VehicleType);
            orderStatusDetail.VehicleNumber = VehicleNumber.GetValue(orderStatusDetail.VehicleNumber);

            orderStatusDetail.VehicleLatitude = VehicleLatitude ?? orderStatusDetail.VehicleLatitude;
            orderStatusDetail.VehicleLongitude = VehicleLongitude ?? orderStatusDetail.VehicleLongitude;
            orderStatusDetail.Eta = Eta ?? orderStatusDetail.Eta;
        }
    }
}