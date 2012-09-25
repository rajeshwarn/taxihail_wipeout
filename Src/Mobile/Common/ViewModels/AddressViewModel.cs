using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile
{
	public class AddressViewModel
	{
		public AddressViewModel ()
		{
		}

		public Address Address { get; set; }

		public bool ShowRightArrow { get; set; }
		public bool ShowPlusSign { get; set; }
		public bool IsFirst { get; set; }
		public bool IsLast { get; set; }
	}
}

