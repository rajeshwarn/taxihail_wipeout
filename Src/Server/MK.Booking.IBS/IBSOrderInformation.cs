﻿using System;
using System.Linq;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.IBS
{
    /// <summary>
    /// class with all the data about the order when requesting  status of a list of order
    /// </summary>
    public class IBSOrderInformation
    {
        public int IBSOrderId { get; set; }
        public string Status { get; set; }
        public double? VehicleLatitude { get; set; }
        public double? VehicleLongitude { get; set; }
        public double? Toll { get; set; }
        public double? Fare { get; set; }
        public double? Tip { get; set; }
        public double? VAT { get; set; }
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

        public DateTime? Eta { get; set; }

        public override string ToString()
        {
            return Status + " " + FirstName;
        }

        public IBSOrderInformation()
        {
            
        }

        public IBSOrderInformation(TOrderStatus orderInfoFromIBS)
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

            Fare = orderInfoFromIBS.Fare;
            Tip = orderInfoFromIBS.Tips;
            Toll = orderInfoFromIBS.Tolls;
            VAT = 0;

            Eta = orderInfoFromIBS.ETATime.ToDateTime();
        }


        public void Update(OrderStatusDetail orderStatusDetail)
        {
            orderStatusDetail.IBSStatusId = Status;

            orderStatusDetail.DriverInfos.FirstName = FirstName.GetValue(orderStatusDetail.DriverInfos.FirstName);
            orderStatusDetail.DriverInfos.LastName = LastName.GetValue(orderStatusDetail.DriverInfos.LastName);
            orderStatusDetail.DriverInfos.MobilePhone = MobilePhone.GetValue(orderStatusDetail.DriverInfos.MobilePhone);
            orderStatusDetail.DriverInfos.VehicleColor = VehicleColor.GetValue(orderStatusDetail.DriverInfos.VehicleColor);
            orderStatusDetail.DriverInfos.VehicleMake = VehicleMake.GetValue(orderStatusDetail.DriverInfos.VehicleMake);
            orderStatusDetail.DriverInfos.VehicleModel = VehicleModel.GetValue(orderStatusDetail.DriverInfos.VehicleModel);
            orderStatusDetail.DriverInfos.VehicleRegistration = VehicleRegistration.GetValue(orderStatusDetail.DriverInfos.VehicleRegistration);
            orderStatusDetail.DriverInfos.VehicleType = VehicleType.GetValue(orderStatusDetail.DriverInfos.VehicleType);
            orderStatusDetail.VehicleNumber = VehicleNumber.GetValue(orderStatusDetail.VehicleNumber);

            orderStatusDetail.VehicleLatitude = VehicleLatitude ?? orderStatusDetail.VehicleLatitude;
            orderStatusDetail.VehicleLongitude = VehicleLongitude ?? orderStatusDetail.VehicleLongitude;
            orderStatusDetail.Eta = Eta ?? orderStatusDetail.Eta;
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
    }
}