using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class SectionAddressViewModel
	{
		public SectionAddressViewModel ()
		{
		}

		public string SectionTitle { get; set; }
		public IEnumerable<AddressViewModel> Addresses { get; set; }
	}
}

