namespace apcurium.MK.Booking.Events
{
	public class OrderReportCreated : OrderCreated
	{
		public string Error { get; set; }
	}
}