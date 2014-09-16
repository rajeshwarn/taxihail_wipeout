namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class DirectionInfo : BaseDto
    {
        public int? Distance { get; set; }
        public double? Price { get; set; }
        public string FormattedDistance { get; set; }
		public OrderValidationResult ValidationResult { get; set; }

        // Used by the webapp
        public string FormattedPrice { get; set; }

        // Used by the webapp
        public string Eta { get; set; }
    }
}