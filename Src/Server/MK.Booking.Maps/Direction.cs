namespace apcurium.MK.Booking.Maps
{
    public class Direction
    {
        public int? Distance { get; set; }
        public int? Duration { get; set; }
        public double? Price { get; set; }
		public string Eta { get; set; }
        public string FormattedPrice { get; set; }
        public string FormattedDistance { get; set; }
		public bool IsValidEta()
		{
			return Distance.HasValue && Duration.HasValue;
		}
    }
}