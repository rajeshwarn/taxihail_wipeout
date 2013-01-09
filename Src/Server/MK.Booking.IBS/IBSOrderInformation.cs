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
    }
}