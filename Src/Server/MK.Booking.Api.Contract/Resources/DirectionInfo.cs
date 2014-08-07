namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class DirectionInfo : BaseDto
    {
        public int? Distance { get; set; }
        public double? Price { get; set; }
        public string FormattedDistance { get; set; }
    }
}