namespace apcurium.MK.Booking.IBS
{
    public class IbsOrderDetails
    {
        public double? Toll { get; set; }
        public double? Fare { get; set; }
        public double? Tip { get; set; }
        public double? VAT { get; set; }
        public string VehicleNumber { get; set; }
        public string CallNumber { get; set; }
    }
}