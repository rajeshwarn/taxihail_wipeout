using System;
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

        public IBSOrderInformation(TOrderStatus orderInfoFromIBS)
        {
            Status = orderInfoFromIBS.OrderStatus.ToString();
            IBSOrderId = orderInfoFromIBS.OrderID;
            VehicleNumber = orderInfoFromIBS.VehicleNumber == null ? null : orderInfoFromIBS.VehicleNumber.Trim(); ;
            MobilePhone = orderInfoFromIBS.DriverMobilePhone;
            FirstName = orderInfoFromIBS.DriverFirstName;
            LastName = orderInfoFromIBS.DriverLastName;
            VehicleColor = orderInfoFromIBS.VehicleColor;
            VehicleLatitude = orderInfoFromIBS.VehicleCoordinateLat != 0 ? (double?)orderInfoFromIBS.VehicleCoordinateLat : null;
            VehicleLongitude = orderInfoFromIBS.VehicleCoordinateLong != 0 ? (double?)orderInfoFromIBS.VehicleCoordinateLong : null;
            VehicleMake = orderInfoFromIBS.VehicleMake;
            VehicleModel = orderInfoFromIBS.VehicleModel;
            VehicleRegistration = orderInfoFromIBS.VehicleRegistration;
            Fare = orderInfoFromIBS.Fare;
            Tip = orderInfoFromIBS.Tips;
            Toll = orderInfoFromIBS.Tolls;
            Eta = orderInfoFromIBS.ETATime == null || orderInfoFromIBS.ETATime.Year < DateTime.Now.Year ? (DateTime?)null : new DateTime(orderInfoFromIBS.ETATime.Year,
                                                                                                orderInfoFromIBS.ETATime.Month, orderInfoFromIBS.ETATime.Day,
                                                                                                orderInfoFromIBS.ETATime.Hour, orderInfoFromIBS.ETATime.Minute,
                                                                                                orderInfoFromIBS.ETATime.Second);
        }


        public void Update(OrderStatusDetail orderStatusDetail)
        {
            orderStatusDetail.IBSStatusId = Status;
            orderStatusDetail.DriverInfos.FirstName = FirstName;
            orderStatusDetail.DriverInfos.LastName = LastName;
            orderStatusDetail.DriverInfos.MobilePhone = MobilePhone;
            orderStatusDetail.DriverInfos.VehicleColor = VehicleColor;
            orderStatusDetail.DriverInfos.VehicleMake = VehicleMake;
            orderStatusDetail.DriverInfos.VehicleModel = VehicleModel;
            orderStatusDetail.DriverInfos.VehicleRegistration = VehicleRegistration;
            orderStatusDetail.DriverInfos.VehicleType = VehicleType;
            orderStatusDetail.VehicleNumber = VehicleNumber;
            orderStatusDetail.VehicleLatitude = VehicleLatitude;
            orderStatusDetail.VehicleLongitude = VehicleLongitude;
            orderStatusDetail.Eta = Eta;


        }

        public bool IsAssigned 
        { 
            get
            {
                return Status.SoftEqual(VehicleStatuses.Common.Assigned);
            } 
        }

        public bool IsComplete
        {
            get { return VehicleStatuses.DoneStatuses.Any(s => s.SoftEqual(Status)); }
        }
    }
}