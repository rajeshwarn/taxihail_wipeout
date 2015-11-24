namespace apcurium.MK.Booking.Mobile.Models
{
	public class PairedTaxiPosition
	{
		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public double? Orientation { get; set; }

        public string Market { get; set; }
	}
}