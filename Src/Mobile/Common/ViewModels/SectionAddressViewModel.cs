using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class SectionAddressViewModel
	{
		public string SectionTitle { get; set; }
		public IEnumerable<AddressViewModel> Addresses { get; set; }
	}
}

